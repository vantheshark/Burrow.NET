using System;
using System.Collections;
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
                                       new DefaultMessageHandlerFactory(new ConsumerErrorHandler(() => connection.ConnectionFactory, 
                                                                                                 Global.DefaultSerializer, 
                                                                                                 Global.DefaultWatcher), 
                                                                        Global.DefaultSerializer,
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
            Publish(rabbit, priority, null);
        }

        private const string PriorityKey = "Priority";
        private const string RoutingKey = "RoutingKey";
        public void Publish<T>(T rabbit, uint priority, IDictionary customHeaders)
        {
            lock (_tunnelGate)
            {
                EnsurePublishChannelIsCreated();
            }

            try
            {
                //NOTE: Routing key is ignored for Headers exchange anyway
                var routingKey = _routeFinder.FindRoutingKey<T>();
                byte[] msgBody = _serializer.Serialize(rabbit);
                IBasicProperties properties = CreateBasicPropertiesForPublishing<T>();
                properties.Priority = (byte)priority;
                properties.Headers = new HybridDictionary();
                properties.Headers.Add(PriorityKey, priority.ToString(CultureInfo.InvariantCulture));
                properties.Headers.Add(RoutingKey, routingKey);

                if (customHeaders != null)
                {
                    foreach (var key in customHeaders.Keys)
                    {
                        if (key == null || customHeaders[key] == null)
                        {
                            continue;
                        }
                        if (PriorityKey.Equals(key.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            //NOTE: Do not overwrite the priority value
                            continue;
                        }

                        properties.Headers.Add(key.ToString(), customHeaders[key].ToString());
                    }
                }
                var exchangeName = _routeFinder.FindExchangeName<T>();
                
                lock (_tunnelGate)
                {
                    _dedicatedPublishingChannel.BasicPublish(exchangeName, routingKey, properties, msgBody);
                }
                
                if (_watcher.IsDebugEnable)
                {
                    _watcher.DebugFormat("Published to {0}, CorrelationId {1}", exchangeName, properties.CorrelationId);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Publish failed: '{0}'", ex.Message), ex);
            }
        }

        public CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateConsumer(channel, subscriptionName, onReceiveMessage);
            return CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer, comparerType);
        }

        public CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateConsumer(channel, subscriptionName, onReceiveMessage);
            return CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer, comparerType);
        }

        public CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null, ushort? batchSize = null)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateAsyncConsumer(channel, subscriptionName, onReceiveMessage, batchSize);
            return CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer, comparerType);
        }

        public CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null, ushort? batchSize = null)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateAsyncConsumer(channel, subscriptionName, onReceiveMessage, batchSize);
            return CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer, comparerType);
        }

        public uint GetMessageCount<T>(string subscriptionName, uint maxPriorityLevel)
        {
            uint count = 0;
            try
            {
                lock (_tunnelGate)
                {
                    EnsurePublishChannelIsCreated();
                    for (uint level = 0; level <= maxPriorityLevel; level++)
                    {
                        var queueName = GetPriorityQueueName<T>(subscriptionName, level);
                        var result = _dedicatedPublishingChannel.QueueDeclarePassive(queueName);
                        if (result != null)
                        {
                            count += result.MessageCount;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
            return count;
        }

        private CompositeSubscription CreateSubscription<T>(string subscriptionName, uint maxPriorityLevel, Func<IModel, string, IBasicConsumer> createConsumer, Type comparerType)
        {
            var comparer = TryGetComparer(comparerType);
            var compositeSubscription = new CompositeSubscription();
            var maxSize = Global.PreFetchSize * ((int)maxPriorityLevel + 1);
            var priorityQueue = new InMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>(maxSize, comparer);
            var sharedSemaphore = string.Format("{0}{1}", subscriptionName, Guid.NewGuid());
            for (uint level = maxPriorityLevel; level >= 0 && level < uint.MaxValue ; level--)
            {
                var subscription = new Subscription { SubscriptionName = subscriptionName };
                uint priority = level;
                var id = Guid.NewGuid();

                Action subscriptionAction = () =>
                {
                    subscription.QueueName = GetPriorityQueueName<T>(subscriptionName, priority);
                    if (string.IsNullOrEmpty(subscription.ConsumerTag))
                    {
                        // Keep the key here because it's used for the key indexes of internal cache
                        subscription.ConsumerTag = string.Format("{0}-{1}", subscriptionName, Guid.NewGuid());
                    }
                    var channel = _connection.CreateChannel();
                    channel.ModelShutdown += (c, reason) =>
                    {
                        if (_disposed) return;
                        RaiseConsumerDisconnectedEvent(subscription);
                        TryReconnect(c, id, reason); 
                    };
                    if (Global.PreFetchSize <= ushort.MaxValue)
                    {
                        channel.BasicQos(0, (ushort)Global.PreFetchSize, false);
                    }
                    else
                    {
                        _watcher.WarnFormat("The prefetch size is too high {0}, the queue will prefetch all the msgs", Global.PreFetchSize);
                    }

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

        private string GetPriorityQueueName<T>(string subscriptionName, uint priority)
        {
            return _routeFinder.FindQueueName<T>(subscriptionName) + PriorityQueuesRabbitSetup.GlobalPriorityQueueSuffix.Get(typeof(T), priority);
        }

        private IComparer<GenericPriorityMessage<BasicDeliverEventArgs>> TryGetComparer(Type comparerType)
        {
            try
            {
                var type = comparerType ?? typeof(PriorityComparer<>);
                var t = type.MakeGenericType(typeof(GenericPriorityMessage<BasicDeliverEventArgs>));
                return (IComparer<GenericPriorityMessage<BasicDeliverEventArgs>>)Activator.CreateInstance(t);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("comparerType must be assignable from IComparer<>", "comparerType", ex);
            }
        }

        protected override void DisposeConsumerManager()
        {
            base.DisposeConsumerManager();
            _priorityConsumerManager.Dispose();
        }
    }
}
