using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Burrow.Extras.Internal
{
    internal class PriorityBurrowConsumer : DefaultBasicConsumer, IDisposable
    {
        internal static object SyncRoot = new object();

        private readonly IRabbitWatcher _watcher;
        private readonly IMessageHandler _messageHandler;
        private readonly bool _autoAck;
        private readonly int _batchSize;

        private bool _channelShutdown;
        
        private Semaphore _pool;
        private CompositeSubscription _subscription;
        internal IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>> PriorityQueue;
        private uint _queuePriorirty;
        private string _sharedSemaphore;


        public PriorityBurrowConsumer(IModel channel, IMessageHandler messageHandler, IRabbitWatcher watcher, bool autoAck, int batchSize)
            : base(channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }

            if (batchSize < 1)
            {
                throw new ArgumentException("batchSize must be greater than or equal 1", "batchSize");
            }

            Model.ModelShutdown += WhenChannelShutdown;
            Model.BasicRecoverAsync(true);

            _messageHandler = messageHandler;
            _messageHandler.HandlingComplete += MessageHandlerHandlingComplete;
            _watcher = watcher;
            _autoAck = autoAck;
            _batchSize = batchSize;
        }

        ///<summary>Overrides DefaultBasicConsumer's OnCancel
        ///implementation, extending it to call the Close() method of
        ///the PriorityQueue.</summary>
        public override void OnCancel()
        {
            if (PriorityQueue != null)
            {
                PriorityQueue.Close();
            }
            base.OnCancel();
        }

        ///<summary>Overrides DefaultBasicConsumer's
        ///HandleBasicDeliver implementation, building a
        ///BasicDeliverEventArgs instance and placing it in the
        ///Priority Queue.</summary>
        public override void HandleBasicDeliver(string consumerTag,
                                                ulong deliveryTag,
                                                bool redelivered,
                                                string exchange,
                                                string routingKey,
                                                IBasicProperties properties,
                                                byte[] body)
        {
            //NOTE: This method is blocked by the RabbitMQ.Client if the unacked messages reach the prefetch size
            var e = new BasicDeliverEventArgs
            {
                ConsumerTag = consumerTag,
                DeliveryTag = deliveryTag,
                Redelivered = redelivered,
                Exchange = exchange,
                RoutingKey = routingKey,
                BasicProperties = properties,
                Body = body
            };
            PriorityQueue.Enqueue(new GenericPriorityMessage<BasicDeliverEventArgs>(e, _queuePriorirty));
        }

        public void Init(IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>> priorityQueue, CompositeSubscription subscription, uint priority, string sharedSemaphore)
        {
            if (priorityQueue == null)
            {
                throw new ArgumentNullException("priorityQueue");
            }
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }
            if (string.IsNullOrEmpty(sharedSemaphore))
            {
                throw new ArgumentNullException("sharedSemaphore");
            }

            _queuePriorirty = priority;
            _subscription = subscription;
            _sharedSemaphore = sharedSemaphore;
            PriorityQueue = priorityQueue;
        }

        public void Ready()
        {
            if (_subscription == null)
            {
                throw new Exception("Subscription not initialized, call Init first");
            }
            if (PriorityQueue == null)
            {
                throw new Exception("PriorityQueue not initialized, call Init first");
            }

            lock (SyncRoot)
            {
                try
                {
                    _pool = Semaphore.OpenExisting(_sharedSemaphore);
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    _pool = new Semaphore(_batchSize, _batchSize, _sharedSemaphore);
                }
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.CurrentThread.Name = string.Format("Consumer thread: {0}, Priority queue: {1}", ConsumerTag, _queuePriorirty);
                    while (!_disposed && !_channelShutdown)
                    {
                        _pool.WaitOne();
                        var msg = PriorityQueue.Dequeue();
                        if (msg != null && msg.Message != null)
                        {
                            _messageHandler.BeforeHandlingMessage(this, msg.Message);
                            HandleMessageDelivery(msg.Message);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    _watcher.WarnFormat("The consumer thread {0} on queue {1} is aborted", ConsumerTag, _queuePriorirty);
                }
            });
        }

        protected internal void MessageHandlerHandlingComplete(BasicDeliverEventArgs eventArgs)
        {
            _pool.Release();
            if (_autoAck)
            {
                DoAck(eventArgs);
            }
        }

        protected internal void WhenChannelShutdown(IModel model, ShutdownEventArgs reason)
        {
            PriorityQueue.Close();
            _channelShutdown = true;
            _watcher.WarnFormat("Channel on queue {0} P:{1} is shutdown: {2}", ConsumerTag, _queuePriorirty, reason.ReplyText);
        }

        protected internal void HandleMessageDelivery(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                #if DEBUG                
                var priority = PriorityMessageHandler.GetMsgPriority(basicDeliverEventArgs);
                _watcher.DebugFormat("Received CId: {0}, RKey: {1}, DTag: {2}, P: {3}", basicDeliverEventArgs.BasicProperties.CorrelationId, basicDeliverEventArgs.RoutingKey, basicDeliverEventArgs.DeliveryTag, priority);
                #endif
                _messageHandler.HandleMessage(this, basicDeliverEventArgs);
            }
            catch (Exception exception)
            {
                _messageHandler.HandleError(this, basicDeliverEventArgs, exception);
                if (_autoAck)
                {
                    DoAck(basicDeliverEventArgs);
                }
                _pool.Release();
            }
        }

        protected internal void DoAck(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            if (_disposed)
            {
                return;
            }

            const string failedToAckMessage = "Basic ack failed because chanel was closed with message {0}. " +
                                              "Message remains on RabbitMQ and will be retried.";

            try
            {
                _subscription.Ack(basicDeliverEventArgs.ConsumerTag, basicDeliverEventArgs.DeliveryTag);
            }
            catch (AlreadyClosedException alreadyClosedException)
            {
                _watcher.WarnFormat(failedToAckMessage, alreadyClosedException.Message);
            }
            catch (IOException ioException)
            {
                _watcher.WarnFormat(failedToAckMessage, ioException.Message);
            }
        }

        private volatile bool _disposed;
        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            try
            {
                _pool.Dispose();
            }
            catch
            {
            }

            PriorityQueue.Close();
        }
    }
}
