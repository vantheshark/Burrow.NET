using System;
using System.Collections.Generic;
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
                                       new DefaultMessageHandlerFactory(new ConsumerErrorHandler(connection, 
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
        public void Publish<T>(T rabbit, uint priority, IDictionary<string, object> customHeaders)
        {
            try
            {
				//NOTE: Routing key is ignored for Headers exchange anyway
				var routingKey = _routeFinder.FindRoutingKey<T>();
                var msgBody = _serializer.Serialize(rabbit);
                var properties = CreateBasicPropertiesForPublishing<T>();
                properties.Priority = (byte)priority;
                properties.Headers = new Dictionary<string, object>
                {
                    {PriorityKey, priority.ToString(CultureInfo.InvariantCulture)},
                    {RoutingKey, routingKey}
                };
				if (customHeaders != null)
                {
                    foreach (var key in customHeaders.Keys)
                    {
                        if (key == null || customHeaders[key] == null)
                        {
                            continue;
                        }
                        if (PriorityKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            //NOTE: Do not overwrite the priority value
                            continue;
                        }

                        properties.Headers.Add(key, customHeaders[key].ToString());
                    }
                }

                var exchangeName = _routeFinder.FindExchangeName<T>();
                lock (_tunnelGate)
                {
                    DedicatedPublishingChannel.BasicPublish(exchangeName, routingKey, properties, msgBody);
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

        public CompositeSubscription Subscribe<T>(PrioritySubscriptionOption<T> subscriptionOption)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateConsumer(channel, subscriptionOption.SubscriptionName, subscriptionOption.MessageHandler, subscriptionOption.BatchSize <= 0 ? (ushort)1 : subscriptionOption.BatchSize);
            return CreateSubscription<T>(subscriptionOption, createConsumer);
        }

        public CompositeSubscription SubscribeAsync<T>(PriorityAsyncSubscriptionOption<T> subscriptionOption)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _priorityConsumerManager.CreateAsyncConsumer(channel, subscriptionOption.SubscriptionName, subscriptionOption.MessageHandler, subscriptionOption.BatchSize <= 0 ? (ushort)1 : subscriptionOption.BatchSize);
            return CreateSubscription<T>(subscriptionOption, createConsumer);
        }

        public CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null)
        {
            return Subscribe(new PrioritySubscriptionOption<T>
            {
                SubscriptionName = subscriptionName,
                MaxPriorityLevel = maxPriorityLevel,
                MessageHandler = onReceiveMessage,
                ComparerType = comparerType,
                BatchSize = 1,
                QueuePrefetchSize = Global.PreFetchSize,
            });
        }

        public CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null)
        {
            return SubscribeAsync(new PriorityAsyncSubscriptionOption<T>
            {
                SubscriptionName = subscriptionName,
                MaxPriorityLevel = maxPriorityLevel,
                MessageHandler = onReceiveMessage,
                ComparerType = comparerType,
                BatchSize = 1,
                QueuePrefetchSize = Global.PreFetchSize,
            });
        }

        public CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null, ushort? batchSize = null)
        {
            return Subscribe(new PrioritySubscriptionOption<T>
            {
                SubscriptionName = subscriptionName,
                MaxPriorityLevel = maxPriorityLevel,
                MessageHandler = onReceiveMessage,
                ComparerType = comparerType,
                BatchSize = batchSize ?? Global.DefaultConsumerBatchSize,
                QueuePrefetchSize = Global.PreFetchSize,
            });
        }

        public CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null, ushort? batchSize = null)
        {
            return SubscribeAsync(new PriorityAsyncSubscriptionOption<T>
            {
                SubscriptionName = subscriptionName,
                MaxPriorityLevel = maxPriorityLevel,
                MessageHandler = onReceiveMessage,
                ComparerType = comparerType,
                BatchSize = batchSize ?? Global.DefaultConsumerBatchSize,
                QueuePrefetchSize = Global.PreFetchSize,
            });
        }

        public uint GetMessageCount<T>(PrioritySubscriptionOption<T> subscriptionOption)
        {
            uint count = 0;
            try
            {
                lock (_tunnelGate)
                {
                    for (uint level = 0; level <= subscriptionOption.MaxPriorityLevel; level++)
                    {
                        var queueName = GetPriorityQueueName<T>(subscriptionOption, level);
                        var result = DedicatedPublishingChannel.QueueDeclarePassive(queueName);
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

        private ushort GetProperPrefetchSize(IPrioritySubscriptionOption subscriptionOption, uint priority)
        {
            var prefetchSize = subscriptionOption.QueuePrefetchSizeSelector != null
                             ? subscriptionOption.QueuePrefetchSizeSelector(priority)
                             : subscriptionOption.QueuePrefetchSize;

            if (prefetchSize <= 0)
            {
                prefetchSize = Global.PreFetchSize;
            }

            if (prefetchSize > ushort.MaxValue)
            {
                _watcher.WarnFormat("The prefetch size is too high {0}, the queue will prefetch the maximum {1} msgs", prefetchSize, ushort.MaxValue);
            }

            return (ushort)Math.Min(ushort.MaxValue, prefetchSize);
        }

        private CompositeSubscription CreateSubscription<T>(IPrioritySubscriptionOption subscriptionOption, Func<IModel, string, IBasicConsumer> createConsumer)
        {
            var comparer = TryGetComparer(subscriptionOption.ComparerType);
            var compositeSubscription = new CompositeSubscription();

            uint maxSize = 0;
            for (uint level = 0; level <= subscriptionOption.MaxPriorityLevel; level++)
            {
                maxSize += GetProperPrefetchSize(subscriptionOption, level);
            }
            var priorityQueue = new InMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>(maxSize, comparer);

            var sharedSemaphore = string.Format("{0}{1}", subscriptionOption.SubscriptionName, Guid.NewGuid());
            for (uint level = 0; level <= subscriptionOption.MaxPriorityLevel; level++)
            {
                var subscription = new Subscription { SubscriptionName = subscriptionOption.SubscriptionName };
                uint priority = level;
                var id = Guid.NewGuid();

                Action subscriptionAction = () =>
                {
                    subscription.QueueName = GetPriorityQueueName<T>(subscriptionOption, priority);
                    if (string.IsNullOrEmpty(subscription.ConsumerTag))
                    {
                        // Keep the key here because it's used for the key indexes of internal cache
                        subscription.ConsumerTag = string.Format("{0}-{1}", subscriptionOption.SubscriptionName, Guid.NewGuid());
                    }
                    var channel = _connection.CreateChannel();
                    channel.ModelShutdown += (c, reason) =>
                    {
                        RaiseConsumerDisconnectedEvent(subscription);
                        TryReconnect(c, id, reason); 
                    };

                    var prefetchSize = GetProperPrefetchSize(subscriptionOption, priority);
                    channel.BasicQos(0, prefetchSize, false);

                    _createdChannels.Add(channel);

                    var consumer = createConsumer(channel, subscription.ConsumerTag);
                    var priorityConsumer = consumer as PriorityBurrowConsumer;
                    if (priorityConsumer == null)
                    {
                        throw new NotSupportedException(string.Format("Expected PriorityBurrowConsumer but was {0}", consumer == null ? "NULL" : consumer.GetType().Name));
                    }
                    
                    priorityConsumer.Init(priorityQueue, compositeSubscription, priority, sharedSemaphore);
                    priorityConsumer.ConsumerTag = subscription.ConsumerTag;
                    Subscription.OutstandingDeliveryTags[subscription.ConsumerTag] = new List<ulong>();
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

        private string GetPriorityQueueName<T>(IPrioritySubscriptionOption subscriptionOption, uint priority)
        {
            return (subscriptionOption.RouteFinder ?? _routeFinder).FindQueueName<T>(subscriptionOption.SubscriptionName) +
                   (subscriptionOption.QueueSuffixNameConvention ?? PriorityQueuesRabbitSetup.GlobalPriorityQueueSuffix).Get(typeof(T), priority);
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
