using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Burrow.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Burrow
{
    /// <summary>
    /// This class responsible for publishing msgs and subsribe to queues
    /// </summary>
    public class RabbitTunnel : ITunnel
    {
        protected readonly IConsumerManager _consumerManager;
        protected readonly IRabbitWatcher _watcher;
        protected readonly IDurableConnection _connection;
        private readonly ICorrelationIdGenerator _correlationIdGenerator;

        protected readonly List<IModel> _createdChannels = new List<IModel>();
        protected readonly ConcurrentDictionary<Guid, Action> _subscribeActions;

        protected static readonly object _tunnelGate = new object();
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(true);

        protected ISerializer _serializer;
        protected IRouteFinder _routeFinder;
        
        private bool _setPersistent;
        protected volatile bool _disposed;


        /// <summary>
        /// This event will be fired once a connection to server is established
        /// </summary>
        public event Action OnOpened;

        /// <summary>
        /// A event to be fired when the tunnel is closed, at this point any activities such as ack/nack can't be used because there is no connection
        /// </summary>
        public event Action OnClosed;

        /// <summary>
        /// This event will be fired once a consumer is disconnected, for example you ack a msg with wrong delivery id (I blame RabbitMQ.Client guys)
        /// </summary>
        public event Action<Subscription> ConsumerDisconnected;

        /// <summary>
        /// Create a tunnel by <see cref="routeFinder"/> and <see cref="IDurableConnection"/>
        /// </summary>
        /// <param name="routeFinder"></param>
        /// <param name="connection"></param>
        public RabbitTunnel(IRouteFinder routeFinder,
                            IDurableConnection connection)
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

        /// <summary>
        /// Create a tunnel by <see cref="IConsumerManager"/>, <see cref="IRouteFinder"/>, <see cref="IDurableConnection"/>, <see cref="ISerializer"/> and <see cref="ICorrelationIdGenerator"/>
        /// </summary>
        /// <param name="consumerManager"></param>
        /// <param name="watcher"></param>
        /// <param name="routeFinder"></param>
        /// <param name="connection"></param>
        /// <param name="serializer"></param>
        /// <param name="correlationIdGenerator"></param>
        /// <param name="setPersistent"></param>
        public RabbitTunnel(IConsumerManager consumerManager,
                            IRabbitWatcher watcher,
                            IRouteFinder routeFinder,
                            IDurableConnection connection, 
                            ISerializer serializer, 
                            ICorrelationIdGenerator correlationIdGenerator,
                            bool setPersistent)
        {
            if (consumerManager == null)
            {
                throw new ArgumentNullException("consumerManager");
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (correlationIdGenerator == null)
            {
                throw new ArgumentNullException("correlationIdGenerator");
            }

            _consumerManager = consumerManager;
            _watcher = watcher;
            _connection = connection;
            _correlationIdGenerator = correlationIdGenerator;
            _observers = new ConcurrentBag<IObserver<ISerializer>>();

            SetRouteFinder(routeFinder);
            SetSerializer(serializer);
            SetPersistentMode(setPersistent);

            _connection.Connected += OpenTunnel;
            _connection.Disconnected += CloseTunnel;
            _subscribeActions = new ConcurrentDictionary<Guid, Action>();
        }

        private void CloseTunnel()
        {
            if (_disposed)
            {
                return;
            }

            _autoResetEvent.WaitOne();
            try
            {
                if (_dedicatedPublishingChannel != null)
                {
                    _dedicatedPublishingChannel.BasicAcks -= OnBrokerReceivedMessage;
                    _dedicatedPublishingChannel.BasicNacks -= OnBrokerRejectedMessage;
                    _dedicatedPublishingChannel.BasicReturn -= OnMessageIsUnrouted;
                }

                _consumerManager.ClearConsumers();

                //NOTE: Sometimes, disposing the channel blocks current thread
                var task = Task.Factory.StartNew(() => _createdChannels.ForEach(DisposeChannel), Global.DefaultTaskCreationOptionsProvider());
                task.ContinueWith(t => _createdChannels.Clear(), Global.DefaultTaskContinuationOptionsProvider());

                if (OnClosed != null)
                {
                    OnClosed();
                }
            }
            finally
            {
                _autoResetEvent.Set();
            }
        }

        private void OpenTunnel()
        {
            _autoResetEvent.WaitOne();
            try
            {
                CreatePublishChannel();
                if (_subscribeActions.Count > 0)
                {
                    _watcher.InfoFormat("Subscribe to queues");
                }
                foreach (var subscription in _subscribeActions.Values)
                {
                    TrySubscribe(subscription);
                }
                if (OnOpened != null)
                {
                    OnOpened();
                }
            }
            finally
            {
                _autoResetEvent.Set();
            }
        }

        protected virtual void CreatePublishChannel()
        {
            if (_dedicatedPublishingChannel == null || !_dedicatedPublishingChannel.IsOpen)
            {
                _watcher.InfoFormat("Creating dedicated publishing channel");
                _dedicatedPublishingChannel = _connection.CreateChannel();
                
                // If still failed, it's time to throw exception
                if (_dedicatedPublishingChannel == null || !_dedicatedPublishingChannel.IsOpen)
                {
                    throw new Exception("No channel to rabbit server established.");
                }

                _createdChannels.Add(_dedicatedPublishingChannel);

                _dedicatedPublishingChannel.BasicAcks += OnBrokerReceivedMessage;
                _dedicatedPublishingChannel.BasicNacks += OnBrokerRejectedMessage;
                _dedicatedPublishingChannel.BasicReturn += OnMessageIsUnrouted;
                _dedicatedPublishingChannel.ModelShutdown += (channel, reason) => _watcher.WarnFormat("Dedicated publishing channel is shutdown: {0}", reason.ReplyText);
                
                _watcher.InfoFormat("Dedicated publishing channel established");
            }
        }

        /// <summary>
        /// Note that for unroutable messages are not considered failures and are both Basic.Return’ed and
        /// Basic.Ack’ed. So, if the "mandatory" or "immediate" are used, the client must also listen for returns
        /// by setting the IModel.BasicReturn handler.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="args"></param>
        [ExcludeFromCodeCoverage]
        protected virtual void OnMessageIsUnrouted(IModel model, RabbitMQ.Client.Events.BasicReturnEventArgs args)
        {
        }

        /// <summary>
        /// If a broker rejects a message via the BasicNacks handler, the publisher may assume that the message
        /// was lost or otherwise undeliverable.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="args"></param>
        [ExcludeFromCodeCoverage]
        protected virtual void OnBrokerRejectedMessage(IModel model, RabbitMQ.Client.Events.BasicNackEventArgs args)
        {
        }

        /// <summary>
        /// Once a broker acknowledges a message via the BasicAcks handler, it has taken responsibility for
        /// keeping the message on disk and on the target queue until some other application retrieves and
        /// acknowledges the message.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="args"></param>
        [ExcludeFromCodeCoverage]
        protected virtual void OnBrokerReceivedMessage(IModel model, RabbitMQ.Client.Events.BasicAckEventArgs args)
        {
        }

        /// <summary>
        /// Bear in mind that the connection may be established before somewhere in the application
        /// Burrow tries to ensure only 1 and 1 connection to server for 1 AppDomain
        /// </summary>
        public bool IsOpened
        {
            get
            {
                return _connection != null && _connection.IsConnected;
            }
        }

        private IModel _dedicatedPublishingChannel;
        public IModel DedicatedPublishingChannel 
        { 
            get
            {
                lock (_tunnelGate)
                {
                    EnsurePublishChannelIsCreated();
                    return _dedicatedPublishingChannel;
                }
            }
        }

        public void Publish<T>(T rabbit)
        {
            Publish(rabbit, _routeFinder.FindRoutingKey<T>(), null);
        }

        public virtual void Publish<T>(T rabbit, string routingKey)
        {
            Publish(rabbit, routingKey, null);
        }

        public void Publish<T>(T rabbit, IDictionary<string, object> customHeaders)
        {
            Publish(rabbit, _routeFinder.FindRoutingKey<T>(), customHeaders);
        }

        private void Publish<T>(T rabbit, string routingKey, IDictionary<string, object> customHeaders)
        {
            try
            {
                byte[] msgBody = _serializer.Serialize(rabbit);
                
                IBasicProperties properties = CreateBasicPropertiesForPublishing<T>();
                if (customHeaders != null)
                {
                    properties.Headers = new Dictionary<string, object>();
                    foreach (var key in customHeaders.Keys)
                    {
                        if (key == null || customHeaders[key] == null)
                        {
                            continue;
                        }
                        properties.Headers.Add(key, customHeaders[key]);
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

        protected void EnsurePublishChannelIsCreated()
        {
            if (!IsOpened)
            {
                // NOTE:  Due to the implementation of IsOpened (DurableConnection.IsConnected), the _dedicatedPublishingChannel could be null because the RabbitMQ connection is possibly establised by a different instance of RabbitTunnel
                _connection.Connect();
            }

            // NOTE: After connect, the _dedicatedPublishingChannel will be created synchronously.
            // If for above reason, this channel has not been created, we can create it here
            if (_dedicatedPublishingChannel == null || !_dedicatedPublishingChannel.IsOpen)
            {
                CreatePublishChannel();
            }
        }

        protected virtual IBasicProperties CreateBasicPropertiesForPublishing<T>()
        {
            IBasicProperties properties = DedicatedPublishingChannel.CreateBasicProperties();
            properties.SetPersistent(_setPersistent); // false = Transient
            properties.Type = Global.DefaultTypeNameSerializer.Serialize(typeof(T));
            properties.CorrelationId = _correlationIdGenerator.GenerateCorrelationId();
            return properties;
        }

        private ushort GetProperPrefetchSize(uint prefetchSize)
        {
            if (prefetchSize > ushort.MaxValue)
            {
                _watcher.WarnFormat("The prefetch size is too high {0}, the queue will prefetch the maximum {1} msgs", prefetchSize, ushort.MaxValue);
            }
            return (ushort)Math.Min(ushort.MaxValue, prefetchSize);
        }

        public Subscription Subscribe<T>(SubscriptionOption<T> subscriptionOption)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateConsumer(channel, subscriptionOption.SubscriptionName, subscriptionOption.MessageHandler, subscriptionOption.BatchSize <= 0 ? (ushort)1 : subscriptionOption.BatchSize);
            var queueName = (subscriptionOption.RouteFinder ?? _routeFinder).FindQueueName<T>(subscriptionOption.SubscriptionName);
            var prefetchSize = GetProperPrefetchSize(subscriptionOption.QueuePrefetchSize);
            return CreateSubscription(subscriptionOption.SubscriptionName, queueName, createConsumer, prefetchSize);
        }

        public Subscription SubscribeAsync<T>(AsyncSubscriptionOption<T> subscriptionOption)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateAsyncConsumer(channel, subscriptionOption.SubscriptionName, subscriptionOption.MessageHandler, subscriptionOption.BatchSize <= 0 ? (ushort)1 : subscriptionOption.BatchSize);
            var queueName = (subscriptionOption.RouteFinder ?? _routeFinder).FindQueueName<T>(subscriptionOption.SubscriptionName);
            var prefetchSize = GetProperPrefetchSize(subscriptionOption.QueuePrefetchSize);
            return CreateSubscription(subscriptionOption.SubscriptionName, queueName, createConsumer, prefetchSize);
        }

        public Subscription Subscribe<T>(string subscriptionName, Action<T> onReceiveMessage)
        {
            return Subscribe(new SubscriptionOption<T>
            {
                SubscriptionName = subscriptionName,
                MessageHandler = onReceiveMessage,
                BatchSize = 1,
                QueuePrefetchSize = Global.PreFetchSize,
            });
        }

        public Subscription Subscribe<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            return SubscribeAsync(new AsyncSubscriptionOption<T>
            {
                SubscriptionName = subscriptionName,
                MessageHandler = onReceiveMessage,
                BatchSize = 1,
                QueuePrefetchSize = Global.PreFetchSize,
            });
        }

        public Subscription SubscribeAsync<T>(string subscriptionName, Action<T> onReceiveMessage, ushort? batchSize)
        {
            return Subscribe(new SubscriptionOption<T>
            {
                SubscriptionName = subscriptionName,
                MessageHandler = onReceiveMessage,
                BatchSize = batchSize ?? Global.DefaultConsumerBatchSize,
                QueuePrefetchSize = Global.PreFetchSize,
            });
        }

        public Subscription SubscribeAsync<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? batchSize)
        {
            return SubscribeAsync(new AsyncSubscriptionOption<T>
            {
                SubscriptionName = subscriptionName,
                MessageHandler = onReceiveMessage,
                BatchSize = batchSize ?? Global.DefaultConsumerBatchSize,
                QueuePrefetchSize = Global.PreFetchSize,
            });
        }

        protected void TryConnectBeforeSubscribing()
        {
            lock (_tunnelGate)
            {
                if (!IsOpened)
                {
                    _connection.Connect();
                }
            }
        }

        protected virtual void TryReconnect(IModel disconnectedChannel, Guid id, ShutdownEventArgs eventArgs)
        {
            _createdChannels.Remove(disconnectedChannel);
            if (eventArgs.ReplyCode == 406 && eventArgs.ReplyText.StartsWith("PRECONDITION_FAILED - unknown delivery tag "))
            {
                _watcher.InfoFormat("Trying to re-subscribe to queue after 2 seconds ...");
                new Timer(subscriptionId => ExecuteSubscription((Guid) subscriptionId), id, 2000, Timeout.Infinite);
            }
        }

        internal void ExecuteSubscription(Guid id)
        {
            try
            {
                _subscribeActions[id]();
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        private Subscription CreateSubscription(string subscriptionName, string queueName, Func<IModel, string, IBasicConsumer> createConsumer, ushort prefetchSize)
        {
            var subscription = new Subscription { SubscriptionName = subscriptionName } ;
            var id = Guid.NewGuid();

            Action subscriptionAction = () =>
            {
                subscription.QueueName = queueName;
                subscription.ConsumerTag = string.Format("{0}-{1}", subscriptionName, Guid.NewGuid());
                var channel = _connection.CreateChannel();
                channel.ModelShutdown += (c, reason) => 
                {
                    if (_disposed) return;
                    RaiseConsumerDisconnectedEvent(subscription);
                    TryReconnect(c, id, reason); 
                };

                channel.BasicQos(0, prefetchSize, false);

                _createdChannels.Add(channel);
                
                subscription.SetChannel(channel);

                var consumer = createConsumer(channel, subscription.ConsumerTag);
                if (consumer is DefaultBasicConsumer)
                {
                    (consumer as DefaultBasicConsumer).ConsumerTag = subscription.ConsumerTag;
                }
                //NOTE: The message will still be on the Unacknowledged list until it's processed and the method
                //      DoAck is call.
                channel.BasicConsume(subscription.QueueName, false /* noAck, must be false */, subscription.ConsumerTag, consumer);
                _watcher.InfoFormat("Subscribed to: {0} with subscriptionName: {1}", subscription.QueueName, subscription.SubscriptionName);
            };

            _subscribeActions[id]= subscriptionAction;
            TrySubscribe(subscriptionAction);
            return subscription;
        }

        protected void RaiseConsumerDisconnectedEvent(Subscription subscription)
        {
            if (ConsumerDisconnected != null)
            {
                ConsumerDisconnected(subscription);
            }
        }

        protected void TrySubscribe(Action subscription)
        {
            try
            {
                subscription();
            }
            catch (OperationInterruptedException)
            {
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        public void SetRouteFinder(IRouteFinder routeFinder)
        {
            if (routeFinder == null)
            {
                throw new ArgumentNullException("routeFinder");
            }
            _routeFinder = routeFinder;
        }
        
        public void SetSerializer(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            _serializer = serializer;
            foreach (var observer in _observers)
            {
                observer.OnNext(serializer);
            }
        }

        public void SetPersistentMode(bool persistentMode)
        {
            _setPersistent = persistentMode;
        }

        public uint GetMessageCount<T>(SubscriptionOption<T> subscriptionOption)
        {
            return GetMessageCount((subscriptionOption.RouteFinder ?? _routeFinder).FindQueueName<T>(subscriptionOption.SubscriptionName));
        }

        public uint GetMessageCount(string queueName)
        {
            try
            {
                lock (_tunnelGate)
                {
                    var result = DedicatedPublishingChannel.QueueDeclarePassive(queueName);
                    if (result != null)
                    {
                        return result.MessageCount;
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
                return 0;
            }
        }

        public void Dispose()
        {
            _disposed = true;
            DisposeConsumerManager();

            //NOTE: Sometimes, disposing the channel blocks current thread
            var task = Task.Factory.StartNew(() => _createdChannels.ForEach(DisposeChannel), Global.DefaultTaskCreationOptionsProvider());
            task.ContinueWith(t => _createdChannels.Clear(), Global.DefaultTaskContinuationOptionsProvider())
                .Wait((int)Global.ConsumerDisposeTimeoutInSeconds * 1000);
            
            if (_connection.IsConnected)
            {
                _connection.Dispose();
            }
        }

		protected virtual void DisposeConsumerManager()
        {
            _consumerManager.Dispose();
        }

        private void DisposeChannel(IModel model)
        {
            if (model != null && model.IsOpen)
            {
                try
                {
                    model.Abort(); // To kill all consumer queues
                    model.Dispose();
                }
                catch(System.IO.IOException)
                {
                    //Channel has been closed by remote host
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }
            }
        }

        /// <summary>
        /// Get the static <see cref="TunnelFactory"/> to create <see cref="ITunnel"/>
        /// </summary>
        public static TunnelFactory Factory { get; internal set; }
        static RabbitTunnel()
        {
            if (Factory == null)
            {
                new TunnelFactory();
            }
        }

        protected readonly ConcurrentBag<IObserver<ISerializer>> _observers;
        
        [ExcludeFromCodeCoverage]
        internal void AddSerializerObserver(IObserver<ISerializer> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
    }
}
