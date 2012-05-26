using System;
using Burrow.Internal;

namespace Burrow.Extras
{
    internal class DependencyInjectionTunnelFactory : TunnelFactory
    {
        private readonly IBurrowResolver _burrowResolver;

        public DependencyInjectionTunnelFactory(IBurrowResolver burrowResolver)
        {
            _burrowResolver = burrowResolver;
        }
        
        public override ITunnel Create()
        {
            return _burrowResolver.Resolve<ITunnel>();
        }

        public override ITunnel Create(string connectionString)
        {
            return Create(connectionString, _burrowResolver.Resolve<IRabbitWatcher>() ?? Global.DefaultWatcher);
        }

        public override ITunnel Create(string hostName, string virtualHost, string username, string password, IRabbitWatcher watcher)
        {
            var rabbitWatcher = watcher ?? _burrowResolver.Resolve<IRabbitWatcher>() ?? Global.DefaultWatcher;
            var serializer = _burrowResolver.Resolve<ISerializer>() ?? Global.DefaultSerializer;
            var connectionFactory = new RabbitMQ.Client.ConnectionFactory
                                        {
                                            HostName = hostName,
                                            VirtualHost = virtualHost,
                                            UserName = username,
                                            Password = password
                                        };
            var durableConnection = new DurableConnection(_burrowResolver.Resolve<IRetryPolicy>() ?? new DefaultRetryPolicy(), 
                                                          rabbitWatcher, 
                                                          connectionFactory);

            Func<IConsumerErrorHandler> errorHandler = () => new ConsumerErrorHandler(connectionFactory,
                                                                                      serializer,
                                                                                      rabbitWatcher);

            Func<IMessageHandlerFactory> handlerFactory = () => new DefaultMessageHandlerFactory(_burrowResolver.Resolve<IConsumerErrorHandler>() ?? errorHandler(), rabbitWatcher);


            Func<IConsumerManager> consumerManager = () => new ConsumerManager(rabbitWatcher,
                                                                               _burrowResolver.Resolve<IMessageHandlerFactory>() ?? handlerFactory(),
                                                                               serializer, 
                                                                               Global.DefaultConsumerBatchSize);

            return new RabbitTunnel(_burrowResolver.Resolve<IConsumerManager>() ?? consumerManager(),
                                    rabbitWatcher,
                                    _burrowResolver.Resolve<IRouteFinder>() ?? new DefaultRouteFinder(),
                                    durableConnection,
                                    serializer,
                                    _burrowResolver.Resolve<ICorrelationIdGenerator>() ?? Global.DefaultCorrelationIdGenerator,
                                    Global.DefaultPersistentMode);
        }
    }
}