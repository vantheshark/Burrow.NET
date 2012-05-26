using Burrow.Internal;

namespace Burrow.Extras.Internal
{
    internal class PriorityTunnelFactory : TunnelFactory
    {
        public PriorityTunnelFactory() : base(false)
        {
        }

        public override ITunnel Create(string hostName, string virtualHost, string username, string password, IRabbitWatcher watcher)
        {
            if (RabbitTunnel.Factory is DependencyInjectionTunnelFactory)
            {
                return RabbitTunnel.Factory.Create(hostName, virtualHost, username, password, watcher);
            }

            var rabbitWatcher = watcher ?? Global.DefaultWatcher;
            var connectionFactory = new RabbitMQ.Client.ConnectionFactory
                                        {
                                            HostName = hostName,
                                            VirtualHost = virtualHost,
                                            UserName = username,
                                            Password = password
                                        };

            var durableConnection = new DurableConnection(new DefaultRetryPolicy(), rabbitWatcher, connectionFactory);
            var errorHandler = new ConsumerErrorHandler(connectionFactory, Global.DefaultSerializer, rabbitWatcher);
            var msgHandlerFactory = new PriorityMessageHandlerFactory(errorHandler, rabbitWatcher);
            var consumerManager = new PriorityConsumerManager(rabbitWatcher, msgHandlerFactory, Global.DefaultSerializer, Global.DefaultConsumerBatchSize);

            return new RabbitTunnelWithPriorityQueuesSupport(consumerManager,
                                                             rabbitWatcher, 
                                                             new DefaultRouteFinder(), 
                                                             durableConnection,
                                                             Global.DefaultSerializer,
                                                             Global.DefaultCorrelationIdGenerator,
                                                             Global.DefaultPersistentMode);
        }
    }
}