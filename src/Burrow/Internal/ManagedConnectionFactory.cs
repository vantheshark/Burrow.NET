using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Burrow.Internal
{
    public class ManagedConnectionFactory : ConnectionFactory
    {
        internal static readonly object SyncConnection = new object();

        public static ManagedConnectionFactory CreateFromConnectionFactory(ConnectionFactory connectionFactory)
        {
            return connectionFactory is ManagedConnectionFactory
                       ? connectionFactory as ManagedConnectionFactory
                       : new ManagedConnectionFactory(connectionFactory);
        }

        public ManagedConnectionFactory()
        {
        }

        public ManagedConnectionFactory(ConnectionString connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            HostName = connectionString.Host;
            Port = connectionString.Port;
            VirtualHost = connectionString.VirtualHost;
            UserName = connectionString.UserName;
            Password = connectionString.Password;
        }

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

        public override IConnection CreateConnection()
        {
            var connection = base.CreateConnection();
            SaveConnection(connection);
            return connection;
        }

        public override IConnection CreateConnection(int maxRedirects)
        {
            var connection = base.CreateConnection(maxRedirects);
            SaveConnection(connection);
            return connection;
        }

        internal void SaveConnection(IConnection connection)
        {
            if (connection != null && connection.IsOpen)
            {
                var key = Endpoint + VirtualHost;
                SharedConnections[key] = connection;
                connection.ConnectionShutdown += ConnectionShutdown;
            }
        }

        private void ConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
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