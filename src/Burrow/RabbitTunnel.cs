using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        protected IModel _dedicatedPublishingChannel;
        private bool _setPersistent;
        
        public event Action OnOpened;
        public event Action OnClosed;
        public event Action<Subscription> ConsumerDisconnected;

        public RabbitTunnel(IRouteFinder routeFinder,
                            IDurableConnection connection)
            : this(new ConsumerManager(Global.DefaultWatcher, 
                                       new DefaultMessageHandlerFactory(new ConsumerErrorHandler(connection.ConnectionFactory, 
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

            SetRouteFinder(routeFinder);
            SetSerializer(serializer);
            SetPersistentMode(setPersistent);

            _connection.Connected += OpenTunnel;
            _connection.Disconnected += CloseTunnel;
            _subscribeActions = new ConcurrentDictionary<Guid, Action>();
        }

        private void CloseTunnel()
        {
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

        private void CreatePublishChannel()
        {
            if (_dedicatedPublishingChannel == null || !_dedicatedPublishingChannel.IsOpen)
            {
                _watcher.InfoFormat("Creating dedicated publishing channel");
                _dedicatedPublishingChannel = _connection.CreateChannel();
                _createdChannels.Add(_dedicatedPublishingChannel);

                _dedicatedPublishingChannel.BasicAcks += OnBrokerReceivedMessage;
                _dedicatedPublishingChannel.BasicNacks += OnBrokerRejectedMessage;
                _dedicatedPublishingChannel.BasicReturn += OnMessageIsUnrouted;
            }
        }

        /// <summary>
        /// Note that for unroutable messages are not considered failures and are both Basic.Return’ed and
        /// Basic.Ack’ed. So, if the "mandatory" or "immediate" are used, the client must also listen for returns
        /// by setting the IModel.BasicReturn handler.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="args"></param>
        protected virtual void OnMessageIsUnrouted(IModel model, RabbitMQ.Client.Events.BasicReturnEventArgs args)
        {
        }

        /// <summary>
        /// If a broker rejects a message via the BasicNacks handler, the publisher may assume that the message
        /// was lost or otherwise undeliverable.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="args"></param>
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

        public void Publish<T>(T rabbit)
        {
            Publish(rabbit, _routeFinder.FindRoutingKey<T>());
        }

        public virtual void Publish<T>(T rabbit, string routingKey)
        {
            lock (_tunnelGate)
            {
                EnsurePublishChannelIsCreated();
            }
            
            try
            {
                byte[] msgBody = _serializer.Serialize(rabbit);
                IBasicProperties properties = CreateBasicPropertiesForPublish<T>();
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

        protected void EnsurePublishChannelIsCreated()
        {
            if (!IsOpened)
            {
                // NOTE:  Due to the implementation of IsOpened, the _dedicatedPublishingChannel could be null because the connection is establised by different instance of RabbitTunnel
                _connection.Connect();
            }

            // NOTE: After connect, the _dedicatedPublishingChannel will be created synchronously.
            // If for above reason, this channel has not been created, we can create it here
            if (_dedicatedPublishingChannel == null || !_dedicatedPublishingChannel.IsOpen)
            {
                OpenTunnel();

                // If still failed, it's time to throw exception
                if (_dedicatedPublishingChannel == null || !_dedicatedPublishingChannel.IsOpen)
                {
                    throw new Exception("No channel to rabbit server established.");
                }
            }
        }

        protected virtual IBasicProperties CreateBasicPropertiesForPublish<T>()
        {
            IBasicProperties properties = _dedicatedPublishingChannel.CreateBasicProperties();
            properties.SetPersistent(_setPersistent); // false = Transient
            properties.Type = Global.DefaultTypeNameSerializer.Serialize(typeof(T));
            properties.CorrelationId = _correlationIdGenerator.GenerateCorrelationId();
            return properties;
        }

        public void Subscribe<T>(string subscriptionName, Action<T> onReceiveMessage)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateConsumer(channel, subscriptionName, onReceiveMessage);
            CreateSubscription<T>(subscriptionName, createConsumer);
        }

        public Subscription Subscribe<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateConsumer(channel, subscriptionName, onReceiveMessage);
            return CreateSubscription<T>(subscriptionName, createConsumer);
        }

        public void SubscribeAsync<T>(string subscriptionName, Action<T> onReceiveMessage, ushort? batchSize)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateAsyncConsumer(channel, subscriptionName, onReceiveMessage, batchSize);
            CreateSubscription<T>(subscriptionName, createConsumer);
        }

        public Subscription SubscribeAsync<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? batchSize)
        {
            TryConnectBeforeSubscribing();
            Func<IModel, string, IBasicConsumer> createConsumer = (channel, consumerTag) => _consumerManager.CreateAsyncConsumer(channel, subscriptionName, onReceiveMessage, batchSize);
            return CreateSubscription<T>(subscriptionName, createConsumer);
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

        private Subscription CreateSubscription<T>(string subscriptionName, Func<IModel, string, IBasicConsumer> createConsumer)
        {
            var subscription = new Subscription { SubscriptionName = subscriptionName } ;
            var id = Guid.NewGuid();
            Action subscriptionAction = () =>
            {
                subscription.QueueName = _routeFinder.FindQueueName<T>(subscriptionName);
                subscription.ConsumerTag = string.Format("{0}-{1}", subscriptionName, Guid.NewGuid());
                var channel = _connection.CreateChannel();
                channel.ModelShutdown += (c, reason) => 
                {
                    RaiseConsumerDisconnectedEvent(subscription);
                    TryReconnect(c, id, reason); 
                };
                channel.BasicQos(0, Global.PreFetchSize, false);
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
        }

        public void SetPersistentMode(bool persistentMode)
        {
            _setPersistent = persistentMode;
        }

        public void Dispose()
        {
            //NOTE: Sometimes, disposing the channel blocks current thread
            var task = Task.Factory.StartNew(() => _createdChannels.ForEach(DisposeChannel), Global.DefaultTaskCreationOptionsProvider());
            task.ContinueWith(t => _createdChannels.Clear(), Global.DefaultTaskContinuationOptionsProvider());
            
            if (_connection.IsConnected)
            {
                _connection.Dispose();
            }
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

        public static TunnelFactory Factory { get; internal set; }
        static RabbitTunnel()
        {
            if (Factory == null)
            {
                new TunnelFactory();
            }
        }
    }
}
