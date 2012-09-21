using System;
using System.Diagnostics.CodeAnalysis;
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

            var abc = new ObjectObserver<IObserver<ISerializer>>();
            
            Func<IConsumerErrorHandler> errorHandler = () =>
            {
                var handdler = new ConsumerErrorHandler(connectionFactory, serializer, rabbitWatcher);
                abc.FireEvent(handdler);
                return handdler;
            };

            Func<IMessageHandlerFactory> handlerFactory = () =>
            {
                var factory = new DefaultMessageHandlerFactory(_burrowResolver.Resolve<IConsumerErrorHandler>() ?? errorHandler(), 
                                                               serializer, 
                                                               rabbitWatcher);
                abc.FireEvent(factory);
                return factory;
            };


            Func<IConsumerManager> consumerManager = () =>
            {
                var manager = new ConsumerManager(rabbitWatcher,
                                                  _burrowResolver.Resolve<IMessageHandlerFactory>() ?? handlerFactory(),
                                                  serializer);
                abc.FireEvent(manager);
                return manager;
            };

            var tunnel = new RabbitTunnel(_burrowResolver.Resolve<IConsumerManager>() ?? consumerManager(),
                                          rabbitWatcher,
                                          _burrowResolver.Resolve<IRouteFinder>() ?? new DefaultRouteFinder(),
                                          durableConnection,
                                          serializer,
                                          _burrowResolver.Resolve<ICorrelationIdGenerator>() ?? Global.DefaultCorrelationIdGenerator,
                                          Global.DefaultPersistentMode);

            abc.ObjectCreated += tunnel.AddSerializerObserver;
            
            return tunnel;

        }

        [ExcludeFromCodeCoverage]
        private class ObjectObserver<T>
        {
            public event Action<T> ObjectCreated;
            public void FireEvent(T observer)
            {
                if (ObjectCreated != null)
                {
                    ObjectCreated(observer);
                }
            }
        }
    }
}