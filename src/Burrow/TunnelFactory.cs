using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Burrow.Internal;
using RabbitMQ.Client;

namespace Burrow
{
    /// <summary>
    /// This class is responsible for creating <see cref="ITunnel"/>.
    /// <para>Any derived of this class will automatically registered itself as the default TunnelFactory in the library ;)</para>
    /// </summary>
    public class TunnelFactory 
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TunnelFactory() : this(true)
        {
        }

        internal TunnelFactory(bool setAsDefault)
        {
            if (setAsDefault)
            {
                RabbitTunnel.Factory = this;
            }
        }

        /// <summary>
        /// This method should only be called before close the main app. It will close all openning connections to RabbitMQ server
        /// </summary>
        public void CloseAllConnections()
        {
            ManagedConnectionFactory.CloseAllConnections();
        }
        
        /// <summary>
        /// Create a tunnel from the ConnectionString key = RabbitMQ 
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public virtual ITunnel Create()
        {
            var rabbitConnectionString = ConfigurationManager.ConnectionStrings["RabbitMQ"];
            if (rabbitConnectionString == null)
            {
                throw new Exception(
                    "Could not find a connection string for RabbitMQ. " +
                    "Please add a connection string in the <ConnectionStrings> secion" +
                    "of the application's configuration file. For example: " +
                    "<add name=\"RabbitMQ\" connectionString=\"host=localhost\" />");
            }

            return Create(rabbitConnectionString.ConnectionString);
        }

        /// <summary>
        /// Create a tunnel with the provided connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public virtual ITunnel Create(string connectionString)
        {
            return Create(connectionString, Global.DefaultWatcher ?? new ConsoleWatcher());
        }

        /// <summary>
        /// Create a tunnel with provided connection string and IRabbitWatcher object
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public virtual ITunnel Create(string connectionString, IRabbitWatcher watcher)
        {
            var clusterConnections = connectionString.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
            if (clusterConnections.Length > 1)
            {
                var factories = clusterConnections.Select(x => new ManagedConnectionFactory(new ConnectionString(x)))
                                                  .ToList();

                var rabbitWatcher = watcher ?? Global.DefaultWatcher;
                var haConnection = new HaConnection(new DefaultRetryPolicy(), rabbitWatcher, factories);
                return Create(haConnection, rabbitWatcher);
            }

            var connectionValues = new ConnectionString(connectionString);
            return Create(new ManagedConnectionFactory(connectionValues), watcher);
        }

        /// <summary>
        /// Create a tunnel with provided params
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <param name="virtualHost"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public virtual ITunnel Create(string hostName, int port, string virtualHost, string username, string password, IRabbitWatcher watcher)
        {
            var connectionFactory = new ManagedConnectionFactory
                                        {
                                            HostName = hostName,
                                            Port = port,
                                            VirtualHost = virtualHost,
                                            UserName = username,
                                            Password = password
                                        };
            return Create(connectionFactory, watcher);
        }

        private ITunnel Create(ConnectionFactory connectionFactory, IRabbitWatcher watcher)
        {
            var rabbitWatcher = watcher ?? Global.DefaultWatcher;
            var durableConnection = new DurableConnection(new DefaultRetryPolicy(), rabbitWatcher, connectionFactory);
            return Create(durableConnection, rabbitWatcher);
        }

        private ITunnel Create(IDurableConnection durableConnection, IRabbitWatcher rabbitWatcher)
        {
            var errorHandler = new ConsumerErrorHandler(() => durableConnection.ConnectionFactory, Global.DefaultSerializer, rabbitWatcher);
            var msgHandlerFactory = new DefaultMessageHandlerFactory(errorHandler, Global.DefaultSerializer, rabbitWatcher);
            var consumerManager = new ConsumerManager(rabbitWatcher, msgHandlerFactory, Global.DefaultSerializer);

            var tunnel = new RabbitTunnel(consumerManager,
                                          rabbitWatcher,
                                          new DefaultRouteFinder(),
                                          durableConnection,
                                          Global.DefaultSerializer,
                                          Global.DefaultCorrelationIdGenerator,
                                          Global.DefaultPersistentMode);

            tunnel.AddSerializerObserver(errorHandler);
            tunnel.AddSerializerObserver(msgHandlerFactory);
            tunnel.AddSerializerObserver(consumerManager);
            return tunnel;
        }
    }
}
