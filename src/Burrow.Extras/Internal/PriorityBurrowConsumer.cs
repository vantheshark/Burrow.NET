using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Burrow.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

        private SafeSemaphore _pool;
        private CompositeSubscription _subscription;
        internal IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>> PriorityQueue;
        private uint _queuePriorirty;
        private string _sharedSemaphore;
        private int _messagesInProgressCount;

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
            PriorityQueue.DeleteAll(msg => msg.Priority == priority);
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
                _pool = new SafeSemaphore(_watcher, _batchSize, _batchSize, _sharedSemaphore);
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.CurrentThread.Name = string.Format("Consumer thread: {0}, Priority queue: {1}", ConsumerTag, _queuePriorirty);
                    while (!_disposed && !_channelShutdown)
                    {
                        try
                        {
#if DEBUG
                            _watcher.DebugFormat("1. Wait the semaphore to release");
                            _pool.WaitOne();
                            _watcher.DebugFormat("2. Semaphore released, wait a msg from RabbitMQ. Probably a wait-for-ack message is blocking this");
#else
                            _pool.WaitOne();
#endif
                            var msg = PriorityQueue.Dequeue();
                            
                            if (msg != null && msg.Message != null)
                            {
#if DEBUG
                                _watcher.DebugFormat("3. Msg from RabbitMQ arrived (probably the previous msg has been acknownledged), prepare to handle it");

#endif
                                HandleMessageDelivery(msg.Message);
                            }
                            else
                            {
                                _watcher.ErrorFormat("Msg from RabbitMQ arrived but it's NULL for some reason, properly a serious BUG :D, contact author asap, release the semaphore for other messages");
                                _pool.Release();
                            }
                        }
                        catch (EndOfStreamException) // NOTE: Must keep the consumer thread alive
                        {
                            // This happen when the internal Queue is closed. The root reason could be connection problem
                            Thread.Sleep(100);
#if DEBUG                            
                            _watcher.DebugFormat("EndOfStreamException occurs, release the semaphore for another message");

#endif
                            _pool.Release();
                        }
                        catch (BadMessageHandlerException ex)
                        {
                            _watcher.Error(ex);
                            Dispose();
                        }
                    }
                }
                catch (ThreadStateException tse)
                {
                    _watcher.WarnFormat("The consumer thread {0} on queue {1} got a ThreadStateException: {2}, {3}", ConsumerTag, _queuePriorirty, tse.Message, tse.StackTrace);
                }
                catch(ThreadInterruptedException)
                {
                    _watcher.WarnFormat("The consumer thread {0} on queue {1} is interrupted", ConsumerTag, _queuePriorirty);
                }
                catch (ThreadAbortException)
                {
                    _watcher.WarnFormat("The consumer thread {0} on queue {1} is aborted", ConsumerTag, _queuePriorirty);
                }
            }, TaskCreationOptions.LongRunning);
        }

        internal void MessageHandlerHandlingComplete(BasicDeliverEventArgs eventArgs)
        {
            _pool.Release();
            Interlocked.Decrement(ref _messagesInProgressCount);
            if (_autoAck)
            {
                DoAck(eventArgs);
            }
        }

        internal void WhenChannelShutdown(IModel model, ShutdownEventArgs reason)
        {
            PriorityQueue.Close();
            _channelShutdown = true;
            _watcher.WarnFormat("Channel on queue {0} P:{1} is shutdown: {2}", ConsumerTag, _queuePriorirty, reason.ReplyText);
        }

        internal void HandleMessageDelivery(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
#if DEBUG                
            var priority = PriorityMessageHandler.GetMsgPriority(basicDeliverEventArgs);
            _watcher.DebugFormat("Received CId: {0}, RKey: {1}, DTag: {2}, P: {3}", basicDeliverEventArgs.BasicProperties.CorrelationId, basicDeliverEventArgs.RoutingKey, basicDeliverEventArgs.DeliveryTag, priority);
#endif
                _messageHandler.HandleMessage(basicDeliverEventArgs);
                Interlocked.Increment(ref _messagesInProgressCount);
            }
            catch (Exception ex)
            {
                throw new BadMessageHandlerException(ex);
            }
        }

        internal void DoAck(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            if (_disposed)
            {
                return;
            }

            _subscription.Ack(basicDeliverEventArgs.ConsumerTag, basicDeliverEventArgs.DeliveryTag);
        }

        private volatile bool _disposed;
        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            //NOTE: Wait all current running tasks to finish and after that dispose the objects
            DateTime timeOut = DateTime.Now.AddSeconds(Global.ConsumerDisposeTimeoutInSeconds);
            while (_messagesInProgressCount > 0 && DateTime.Now <= timeOut)
            {
                _watcher.InfoFormat("Wait for {0} messages on queue {1} in progress", _messagesInProgressCount, _queuePriorirty );
                Thread.Sleep(1000);
            }

            _pool.Dispose();
            PriorityQueue.Close();
        }
    }
}
