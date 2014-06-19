using System;
using System.Linq;
using Burrow.Internal;

namespace Burrow.Extras.Internal
{
    internal class PriorityTunnelFactory : TunnelFactory
    {
        /// <summary>
        /// This factory will never overwrite the RabbitTunnel.Factory object
        /// </summary>
        public PriorityTunnelFactory() : base(false)
        {
        }

        public override ITunnel Create(string connectionString, IRabbitWatcher watcher)
        {
            if (RabbitTunnel.Factory is DependencyInjectionTunnelFactory)
            {
                return RabbitTunnel.Factory.Create(connectionString, watcher);
            }

            var clusterConnections = connectionString.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (clusterConnections.Length > 1)
            {
                var factories = clusterConnections.Select(x => new ManagedConnectionFactory(new ConnectionString(x)))
                                                  .ToList();

                var rabbitWatcher = watcher ?? Global.DefaultWatcher;
                var haConnection = new HaConnection(new DefaultRetryPolicy(), rabbitWatcher, factories);
                return Create(haConnection, rabbitWatcher);
            }


            var cnn = new ConnectionString(connectionString);
            return Create(cnn.Host, cnn.Port, cnn.VirtualHost, cnn.UserName, cnn.Password, watcher);
        }

        public override ITunnel Create(string hostName, int port, string virtualHost, string username, string password, IRabbitWatcher watcher)
        {
            if (RabbitTunnel.Factory is DependencyInjectionTunnelFactory)
            {
                return RabbitTunnel.Factory.Create(hostName, port, virtualHost, username, password, watcher);
            }

            var rabbitWatcher = watcher ?? Global.DefaultWatcher;
            var connectionFactory = new ManagedConnectionFactory
                                        {
                                            HostName = hostName,
                                            Port = port,
                                            VirtualHost = virtualHost,
                                            UserName = username,
                                            Password = password
                                        };

            var durableConnection = new DurableConnection(new DefaultRetryPolicy(), rabbitWatcher, connectionFactory);
            

            return Create(durableConnection, rabbitWatcher);
        }

        private ITunnel Create(DurableConnection durableConnection, IRabbitWatcher rabbitWatcher)
        {
            var errorHandler = new ConsumerErrorHandler(durableConnection, Global.DefaultSerializer, rabbitWatcher);
            var msgHandlerFactory = new PriorityMessageHandlerFactory(errorHandler, Global.DefaultSerializer, rabbitWatcher);
            var consumerManager = new ConsumerManager(rabbitWatcher, msgHandlerFactory, Global.DefaultSerializer);

            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(consumerManager,
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