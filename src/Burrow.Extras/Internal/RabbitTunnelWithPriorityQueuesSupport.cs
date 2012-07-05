using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Burrow.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow.Extras.Internal
{
    internal class RabbitTunnelWithPriorityQueuesSupport : RabbitTunnel, ITunnelWithPrioritySupport
    {
        private readonly PriorityConsumerManager _priorityConsumerManager;

        public RabbitTunnelWithPriorityQueuesSupport(IRouteFinder routeFinder, IDurableConnection connection)
            : this(new ConsumerManager(Global.DefaultWatcher, 
                                       new DefaultMessageHandlerFactory(new ConsumerErrorHandler(connection.ConnectionFactory, 
                                                                                                 Global.DefaultSerializer, 
                                                                                                 Global.DefaultWatcher), 
                                                                            Global.DefaultWatcher), 
                                       Global.DefaultSerializer),
                   Global.DefaultWatcher, 
                   routeFinder, 
                   connection,
                   Global.DefaultSerializer, 
                   Global.DefaultCorrelationIdGenerator,
                   Global.DefaultPersistentMode)
        {
        }

        public RabbitTunnelWithPriorityQueuesSupport(IConsumerManager consumerManager,
                                                     IRabbitWatcher watcher,
                                                     IRouteFinder routeFinder,
                                                     IDurableConnection connection,
                                                     ISerializer serializer,
                                                     ICorrelationIdGenerator correlationIdGenerator,
                                                     bool setPersistent)
            : base(consumerManager, watcher, routeFinder, connection, serializer, correlationIdGenerator, setPersistent)
        {
            _priorityConsumerManager = new PriorityConsumerManager(watcher, consumerManager.MessageHandlerFactory, serializer);
        }

        public void Publish<T>(T rabbit, uint priority)
        {
            Publish(rabbit, _routeFinder.FindRoutingKey<T>(), priority);
        }

        public virtual void Publish<T>(T rabbit, string routingKey, uint priority)
        {
            lock (_tunnelGate)
            {
                EnsurePublishChannelIsCreated();
            }

            try
            {
                byte[] msgBody = _serializer.Serialize(rabbit);
                IBasicProperties properties = CreateBasicPropertiesForPublish<T>();
                properties.Priority = (byte)priority;
                properties.Headers = new HybridDictionary();
                properties.Headers.Add("Priority", priority.ToString(CultureInfo.InvariantCulture));
                properties.Headers.Add("RoutingKey", routingKey);

                var exchangeName = _routeFinder.FindExchangeName<T>();
                lock (_tunnelGate)
                {
                    _dedicatedPublishingChannel.BasicPublish(exchangeName, routingKey, properties, msgBody);
                }
                _watcher.DebugFormat("Published to {0}, CorrelationId {1}", exchangeName, properties.CorrelationId);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Publish failed: '{0}'", ex.Message), ex);
            }
        }

        public void Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateConsumer(channel, subscriptionName, onReceiveMessage);
            CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer, comparerType);
        }

        public CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateConsumer(channel, subscriptionName, onReceiveMessage);
            return CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer, comparerType);
        }

        public void SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null, ushort? batchSize = null)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateAsyncConsumer(channel, subscriptionName, onReceiveMessage, batchSize);
            CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer, comparerType);
        }

        public CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null, ushort? batchSize = null)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateAsyncConsumer(channel, subscriptionName, onReceiveMessage, batchSize);
            return CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer, comparerType);
        }

        private CompositeSubscription CreateSubscription<T>(string subscriptionName, uint maxPriorityLevel, Func<IModel, string, IBasicConsumer> createConsumer, Type comparerType)
        {
            var comparer = TryGetComparer(comparerType);
            var compositeSubscription = new CompositeSubscription();
            var maxSize = Global.PreFetchSize * ((int)maxPriorityLevel + 1);
            var priorityQueue = new InMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>(maxSize, comparer);
            var sharedSemaphore = string.Format("{0}{1}", subscriptionName, Guid.NewGuid());
            for (uint level = 0; level <= maxPriorityLevel; level++)
            {
                var subscription = new Subscription { SubscriptionName = subscriptionName };
                uint priority = level;
                var id = Guid.NewGuid();

                Action subscriptionAction = () =>
                {
                    subscription.QueueName = _routeFinder.FindQueueName<T>(subscriptionName) + PriorityQueuesRabbitSetup.GlobalPriorityQueueSuffix.Get(typeof(T), priority);
                    if (string.IsNullOrEmpty(subscription.ConsumerTag))
                    {
                        // Keep the key here because it's used for the key indexes of internal cache
                        subscription.ConsumerTag = string.Format("{0}-{1}", subscriptionName, Guid.NewGuid());
                    }
                    var channel = _connection.CreateChannel();
                    channel.ModelShutdown += (c, reason) =>
                    {
                        RaiseConsumerDisconnectedEvent(subscription);
                        TryReconnect(c, id, reason); 
                        
                    };
                    channel.BasicQos(0, Global.PreFetchSize, false);
                    _createdChannels.Add(channel);

                    var consumer = createConsumer(channel, subscription.ConsumerTag);
                    var priorityConsumer = consumer as PriorityBurrowConsumer;
                    if (priorityConsumer == null)
                    {
                        throw new NotSupportedException(string.Format("Expected PriorityBurrowConsumer but was {0}", consumer == null ? "NULL" : consumer.GetType().Name));
                    }
                    
                    priorityConsumer.Init(priorityQueue, compositeSubscription, priority, sharedSemaphore);
                    priorityConsumer.ConsumerTag = subscription.ConsumerTag;
                    subscription.SetChannel(channel);

                    //NOTE: The message will still be on the Unacknowledged list until it's processed and the method
                    //      DoAck is call.
                    channel.BasicConsume(subscription.QueueName,
                                         false /* noAck, must be false */,
                                         subscription.ConsumerTag, priorityConsumer);
                    _watcher.InfoFormat("Subscribed to: {0} with subscriptionName: {1}",
                                        subscription.QueueName,
                                        subscription.SubscriptionName);
                    priorityConsumer.Ready();
                };

                _subscribeActions[id] = subscriptionAction;
                TrySubscribe(subscriptionAction);
                compositeSubscription.AddSubscription(subscription);
            }
            return compositeSubscription;
        }

        private IComparer<GenericPriorityMessage<BasicDeliverEventArgs>> TryGetComparer(Type comparerType)
        {
            var type = comparerType ?? typeof(PriorityComparer<>);
            if (type.IsAssignableFrom(typeof(IComparer<>)))
            {
                throw new ArgumentException("comparerType must be assignable from IComparer<>", "comparerType");
            }

            var t = type.MakeGenericType(typeof(GenericPriorityMessage<BasicDeliverEventArgs>));
            return (IComparer<GenericPriorityMessage<BasicDeliverEventArgs>>)Activator.CreateInstance(t);
        }
    }
}
