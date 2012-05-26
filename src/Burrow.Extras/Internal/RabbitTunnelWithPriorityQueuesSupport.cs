using System;
using System.Collections.Specialized;
using System.Globalization;
using Burrow.Internal;
using RabbitMQ.Client;

namespace Burrow.Extras.Internal
{
    public class RabbitTunnelWithPriorityQueuesSupport : RabbitTunnel, ITunnelWithPrioritySupport
    {
        public RabbitTunnelWithPriorityQueuesSupport(IRouteFinder routeFinder,
                                                     IDurableConnection connection)
            : this(new PriorityConsumerManager(Global.DefaultWatcher,
                                       new PriorityMessageHandlerFactory(new ConsumerErrorHandler(connection.ConnectionFactory, 
                                                                                                  Global.DefaultSerializer, 
                                                                                                  Global.DefaultWatcher), 
                                                                             Global.DefaultWatcher), 
                                       Global.DefaultSerializer, 
                                       Global.DefaultConsumerBatchSize), 
                   Global.DefaultWatcher, 
                   routeFinder, 
                   connection,
                   Global.DefaultSerializer, 
                   Global.DefaultCorrelationIdGenerator,
                   Global.DefaultPersistentMode)
        {
        }

        public RabbitTunnelWithPriorityQueuesSupport(PriorityConsumerManager consumerManager,
                            IRabbitWatcher watcher,
                            IRouteFinder routeFinder,
                            IDurableConnection connection, 
                            ISerializer serializer, 
                            ICorrelationIdGenerator correlationIdGenerator,
                            bool setPersistent) : base(consumerManager, watcher,routeFinder,connection,serializer,correlationIdGenerator, setPersistent)
        {
        }

        public void Publish<T>(T rabbit, uint priority)
        {
            Publish(rabbit, _routeFinder.FindRoutingKey<T>(), priority);
        }

        public virtual void Publish<T>(T rabbit, string routingKey, uint priority)
        {
            lock (_tunnelGate)
            {
                if (!IsOpened)
                {
                    _connection.Connect();
                }
                //NOTE: After connect, the _dedicatedPublishingChannel will be created synchronously
                if (_dedicatedPublishingChannel == null || !_dedicatedPublishingChannel.IsOpen)
                {
                    throw new Exception("Publish failed. No channel to rabbit server established.");
                }
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

        public void Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateConsumer(channel, subscriptionName, consumerTag, onReceiveMessage);
            CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer);
        }

        public CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateConsumer(channel, subscriptionName, consumerTag, onReceiveMessage);
            return CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer);
        }

        public void SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateAsyncConsumer(channel, subscriptionName, consumerTag, onReceiveMessage);
            CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer);
        }

        public CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateAsyncConsumer(channel, subscriptionName, consumerTag, onReceiveMessage);
            return CreateSubscription<T>(subscriptionName, maxPriorityLevel, createConsumer);
        }

        private CompositeSubscription CreateSubscription<T>(string subscriptionName, uint maxPriorityLevel, Func<IModel, string, IBasicConsumer> createConsumer)
        {
            var result = new CompositeSubscription();
            var sharedEventBroker = new SharedEventBroker(_watcher);
            for (var level = 0; level <= maxPriorityLevel; level++)
            {
                var subscription = new Subscription {SubscriptionName = subscriptionName};
                var priority = level;
                Action subscriptionAction = () =>
                {
                    subscription.QueueName = _routeFinder.FindQueueName<T>(subscriptionName) + "_Priority" + priority;
                    subscription.ConsumerTag = string.Format("{0}-{1}", subscriptionName, Guid.NewGuid());

                    var channel = _connection.CreateChannel();
                    channel.BasicQos(0, Global.DefaultConsumerBatchSize, false);
                    _createdChannels.Add(channel);

                    var consumer = createConsumer(channel, subscription.ConsumerTag);
                    ((PriorityBurrowConsumer)consumer).QueuePriorirty = priority;
                    ((PriorityBurrowConsumer)consumer).SetEventBroker(sharedEventBroker);
                    
                    subscription.SetChannel(channel);

                    //NOTE: The message will still be on the Unacknowledged list until it's processed and the method
                    //      DoAck is call.
                    channel.BasicConsume(subscription.QueueName,
                                         false /* noAck, must be false */,
                                         subscription.ConsumerTag, consumer);
                    _watcher.InfoFormat("Subscribed to: {0} with subscriptionName: {1}",
                                        subscription.QueueName,
                                        subscription.SubscriptionName);
                };

                _subscribeActions.Add(subscriptionAction);
                TrySubscribe(subscriptionAction);
                result.AddSubscription(subscription);
            }
            return result;
        }
    }
}
