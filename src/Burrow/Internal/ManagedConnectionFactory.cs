using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RabbitMQ.Client;

namespace Burrow.Internal
{
    /// <summary>
    /// An event to fire when a physical connection to RabbitMQ server is made.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="virtualHost"></param>
    public delegate void ConnectionEstablished(AmqpTcpEndpoint endpoint, string virtualHost);

    /// <summary>
    /// A simple wrapper of <see cref="ConnectionFactory"/> which will store any created <see cref="IConnection"/>
    /// to memory and share them within the AppDomain.
    /// The purpose of this is keeping the amount of connection to RabbitMQ server as low as possible
    /// </summary>
    public class ManagedConnectionFactory : ConnectionFactory
    {
        internal static readonly object SyncConnection = new object();

        /// <summary>
        /// An event that will fire when a physical connection to RabbitMQ server is made.
        /// <para>The application can use TunnelFactory object to create tunnel manytime, during the creation, many instance of <see cref="ManagedConnectionFactory"/> are created</para>
        /// <para>If the connection to rabbitMQ server lost, the <see cref="IDurableConnection"/> implementation will retry and of of them will make the connection to rabbitMQ server established</para>
        /// <para>All the other <see cref="IDurableConnection"/> instances should also know about this and retry their channels & subscriptions</para>
        /// </summary>
        public static ConnectionEstablished ConnectionEstablished { get; set; }

        /// <summary>
        /// Create a <see cref="ManagedConnectionFactory"/> from a known <see cref="ConnectionFactory"/>
        /// </summary>
        /// <param name="connectionFactory"></param>
        /// <returns></returns>
        public static ManagedConnectionFactory CreateFromConnectionFactory(ConnectionFactory connectionFactory)
        {
            return connectionFactory is ManagedConnectionFactory
                       ? connectionFactory as ManagedConnectionFactory
                       : new ManagedConnectionFactory(connectionFactory);
        }

        /// <summary>
        /// Initialize a <see cref="ManagedConnectionFactory"/>
        /// </summary>
        public ManagedConnectionFactory()
        {
        }

        /// <summary>
        /// Initialize a <see cref="ManagedConnectionFactory"/> from a <see cref="ConnectionString"/> object
        /// </summary>
        public ManagedConnectionFactory(ConnectionString connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            HostName = connectionString.Host;
            Port = connectionString.Port;
            VirtualHost = connectionString.VirtualHost;
            UserName = connectionString.UserName;
            Password = connectionString.Password;
        }

        /// <summary>
        /// Create a <see cref="ManagedConnectionFactory"/> from a known <see cref="ConnectionFactory"/>
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ManagedConnectionFactory(ConnectionFactory factory)
        {
            AuthMechanisms = factory.AuthMechanisms;
            ClientProperties = factory.ClientProperties;
            Endpoint = factory.Endpoint;
            HostName = factory.HostName;
            Password = factory.Password;
            Port = factory.Port;
            Protocol = factory.Protocol;
            RequestedChannelMax = factory.RequestedChannelMax;
            RequestedConnectionTimeout = factory.RequestedConnectionTimeout;
            RequestedFrameMax = factory.RequestedFrameMax;
            RequestedHeartbeat = factory.RequestedHeartbeat;
            SocketFactory = factory.SocketFactory;
            Ssl = factory.Ssl;
            UserName = factory.UserName;
            VirtualHost = factory.VirtualHost;
        }

        public sealed override IConnection CreateConnection()
        {
            var connection = EstablishConnection();
            SaveConnection(connection);
            return connection;
        }

        /// <summary>
        /// Call the base method to establish the connection to RabbitMQ
        /// Burrow.NET uses different way of managing the host addresses in the cluster
        /// TODO: Use <see cref="AmqpTcpEndpoint"/> instead of <see cref="ConnectionString"/>
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        internal protected virtual IConnection EstablishConnection()
        {
            return base.CreateConnection();
        }
       
        private void SaveConnection(IConnection connection)
        {
            if (connection != null && connection.IsOpen)
            {
                var key = Endpoint + VirtualHost;
                SharedConnections[key] = connection;
                connection.ConnectionShutdown += ConnectionShutdown;

                ConnectionEstablished?.Invoke(Endpoint, VirtualHost);
            }
        }

        private void ConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            foreach (var c in SharedConnections)
            {
                if (c.Key == Endpoint + VirtualHost)
                {
                    SharedConnections.Remove(c.Key);
                    break;
                }
            }
        }

        //One AppDomain should create only 1 connection to server except connect to different virtual hosts
        internal static volatile Dictionary<string, IConnection> SharedConnections = new Dictionary<string, IConnection>();
        
        internal static void CloseAllConnections()
        {
            SharedConnections.Values.ToList().ForEach(c =>
            {
                c.Close(200, "Connection disposed by application");
                c.Dispose();
            });
            SharedConnections.Clear();
        }
    }
}