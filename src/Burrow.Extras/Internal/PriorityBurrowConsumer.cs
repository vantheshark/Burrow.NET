using System;
using System.IO;
using System.Threading;
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

            var thread = new Thread(() => 
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
                                _messageHandler.BeforeHandlingMessage(this, msg.Message);
                                HandleMessageDelivery(msg.Message);
                            }
                            else
                            {
                                _watcher.ErrorFormat("3. Msg from RabbitMQ arrived but it's NULL for some reason,  properly a serious BUG :D, contact author asap, release the semaphore for other messages");
                                _pool.Release();
                            }
                        }
                        catch (EndOfStreamException) // NOTE: Must keep the consumer thread alive
                        {
                            // Properly need to end this thread here because the new consumer will be created

                            // do nothing here, EOS fired when queue is closed. Demonstrate that by stop the RabbitMQ service
                            // Looks like the connection has gone away, so wait a little while
                            // before continuing to poll the queue
                            Thread.Sleep(100);
                            _watcher.DebugFormat("EndOfStreamException occurs, release the semaphore for another message");
                            _pool.Release();
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
            });
            thread.IsBackground = true;
            thread.Start();
        }

        protected internal void MessageHandlerHandlingComplete(BasicDeliverEventArgs eventArgs)
        {
            try
            {
                _pool.Release();
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
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
