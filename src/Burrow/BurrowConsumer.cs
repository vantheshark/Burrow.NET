using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Burrow.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;

namespace Burrow
{
    /// <summary>
    /// Inherit from <see cref="QueueingBasicConsumer"/> to handle message using a <see cref="IMessageHandler"/>
    /// </summary>
    public class BurrowConsumer : QueueingBasicConsumer, IDisposable
    {
        /// <summary>
        /// The number of threads to process messages, Default is Global.DefaultConsumerBatchSize
        /// </summary>
        public int BatchSize { get; private set; }
        protected readonly IRabbitWatcher _watcher;
        private readonly bool _autoAck;
        private bool _channelShutdown;

        private readonly object _sharedQueueLock = new object();
        /// <summary>
        /// Control the sharedqueue to receive enough messages to process in parallel if <see cref="BatchSize"/> greater than 1
        /// </summary>
        protected SafeSemaphore _pool { get; set; }
        private readonly IMessageHandler _messageHandler;

        /// <summary>
        /// Initialize an object of <see cref="BurrowConsumer"/>
        /// </summary>
        /// <param name="channel">RabbitMQ.Client channel</param>
        /// <param name="messageHandler">An instance of message handler to handle the message from queue</param>
        /// <param name="watcher"></param>
        /// <param name="autoAck">If set to true, the msg will be acked after processed</param>
        /// <param name="batchSize"></param>
        public BurrowConsumer(IModel channel,
                              IMessageHandler messageHandler,
                              IRabbitWatcher watcher,
                              bool autoAck,
                              int batchSize) : this(channel, messageHandler, watcher, autoAck, batchSize, true)
        {
            // This is the public constructor to start the consuming thread straight away
        }

        /// <summary>
        /// Initialize an object of <see cref="BurrowConsumer"/>
        /// </summary>
        /// <param name="channel">RabbitMQ.Client channel</param>
        /// <param name="messageHandler">An instance of message handler to handle the message from queue</param>
        /// <param name="watcher"></param>
        /// <param name="autoAck">If set to true, the msg will be acked after processed</param>
        /// <param name="batchSize"></param>
        /// <param name="startThread">Whether should start the consuming thread straight away</param>
        protected BurrowConsumer(IModel channel,
                                 IMessageHandler messageHandler,
                                 IRabbitWatcher watcher,
                                 bool autoAck,
                                 int batchSize, bool startThread)
            : base(channel, new SharedQueue<BasicDeliverEventArgs>())
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }
            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
            }

            if (batchSize < 1)
            {
                throw new ArgumentException("batchSize must be greater than or equal 1", nameof(batchSize));
            }

            Model.ModelShutdown += WhenChannelShutdown; ;
            Model.BasicRecoverAsync(true);
            BatchSize = batchSize;

            _pool = new SafeSemaphore(watcher, BatchSize, BatchSize);
            _watcher = watcher;
            _autoAck = autoAck;

            _messageHandler = messageHandler;
            _messageHandler.HandlingComplete += MessageHandlerHandlingComplete;
            _messageHandler.MessageWasNotHandled += MessageWasNotHandled;

            if (startThread)
            {
                StartConsumerThread($"Consumer thread: {ConsumerTag}");
            }
        }

        protected void StartConsumerThread(string threadName)
        {
            ThreadStart startDelegate = () =>
            {
                try
                {
                    Action<BasicDeliverEventArgs> handler = HandleMessageDeliveryInSameThread;
                    if (BatchSize > 1)
                    {
                        handler = HandleMessageDeliveryInSeperatedThread;
                    }

                    while (_status == ConsumerStatus.Active && !_channelShutdown)
                    {
                        WaitAndHandleMessageDelivery(handler);
                    }
                }
                catch (ThreadStateException tse)
                {
                    _watcher.WarnFormat("The consumer thread {0} got a ThreadStateException: {1}, {2}", ConsumerTag, tse.Message, tse.StackTrace);
                }
                catch (ThreadInterruptedException)
                {
                    _watcher.WarnFormat("The consumer thread {0} is interrupted", ConsumerTag);
                }
                catch (ThreadAbortException)
                {
                    _watcher.WarnFormat("The consumer thread {0} is aborted", ConsumerTag);
                }
            };

            var threadOne = new Thread(startDelegate)
            {
                Name = threadName,
                Priority = ThreadPriority.AboveNormal,
                IsBackground = true
            };
            threadOne.Start();
        }

        protected virtual BasicDeliverEventArgs Dequeue()
        {
            return Queue.Dequeue(); 
        }

        protected virtual void CloseQueue()
        {
            Queue.Close();
        }

        internal void WaitAndHandleMessageDelivery(Action<BasicDeliverEventArgs> handler)
        {
            try
            {
                BasicDeliverEventArgs deliverEventArgs = null;
                lock (_sharedQueueLock)
                {
#if DEBUG
                    _watcher.DebugFormat("1. Wait the semaphore to release");
                    _pool.WaitOne();
                    _watcher.DebugFormat("2. Semaphore released, wait a msg from RabbitMQ. Probably a wait-for-ack message is blocking this");
#else
                    _pool.WaitOne();
#endif
					if (_status == ConsumerStatus.Active)
                    {
                        deliverEventArgs = Dequeue();
                    }
#if DEBUG

                    _watcher.DebugFormat("3. Msg from RabbitMQ arrived (probably the previous msg has been acknownledged), prepare to handle it");
#endif
                }
                if (deliverEventArgs != null)
                {
                    lock (Subscription.OutstandingDeliveryTags)
                    {
                        Subscription.OutstandingDeliveryTags[deliverEventArgs.ConsumerTag].Add(deliverEventArgs.DeliveryTag);
                    }

                    handler(deliverEventArgs);
                }
                else
                {
                    _watcher.ErrorFormat("Message arrived but it's null for some reason, properly a serious BUG :D, contact author asap, release semaphore for other messages");
                    _pool.Release();
                }
            }
            catch (EndOfStreamException)
            {
                // This thread will be ended soon because the new consumer will be created
                // do nothing here, EOS fired when queue is closed
                // Looks like the connection has gone away, so wait a little while
                // before continuing to poll the queue
                Thread.Sleep(100);
#if DEBUG
                _watcher.DebugFormat("EndOfStreamException occurs, release the semaphore for another message");

#endif
                _pool.Release();
            }
        }

        private void MessageWasNotHandled(BasicDeliverEventArgs eventArgs)
        {
            try
            {
                if (!_autoAck && !IsDisposed)
                {
                    DoAck(eventArgs, this);
                }
            }
            catch(Exception ex)
            {
                _watcher.Error(ex);
            }
        }


        private void MessageHandlerHandlingComplete(BasicDeliverEventArgs eventArgs)
        {
            try
            {
                if (_autoAck && !IsDisposed)
                {
#if DEBUG
                    _watcher.DebugFormat("7. A task to execute the provided callback with DTag: {0} by CTag: {1} has been finished, now ack message", eventArgs.DeliveryTag, eventArgs.ConsumerTag);

#endif
                    DoAck(eventArgs, this);
                }
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
            finally
            {
#if DEBUG
                _watcher.DebugFormat("6. A task to execute the provided callback with DTag: {0} by CTag: {1} has been finished, now release the semaphore", eventArgs.DeliveryTag, eventArgs.ConsumerTag);

#endif
                _pool.Release();
            }
        }

        protected virtual void WhenChannelShutdown(object sender, ShutdownEventArgs reason)
        {
            lock (Subscription.OutstandingDeliveryTags)
            {
                if (Subscription.OutstandingDeliveryTags.ContainsKey(ConsumerTag))
                {
                    List<ulong> list;
                    Subscription.OutstandingDeliveryTags.TryRemove(ConsumerTag, out list);
                }
            }
            CloseQueue();
            _channelShutdown = true;
            _watcher.WarnFormat("Channel on queue {0} is shutdown: {1}", ConsumerTag, reason.ReplyText);
        }
        
        private void HandleMessageDeliveryInSeperatedThread(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _messageHandler.HandleMessage(basicDeliverEventArgs);
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                    Dispose();
                }
            }, Global.DefaultTaskCreationOptionsProvider());
        }

        private void HandleMessageDeliveryInSameThread(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                _messageHandler.HandleMessage(basicDeliverEventArgs);
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
                Dispose();
            }
        }

        internal protected virtual void DoAck(BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
        {
            if (IsDisposed)
            {
                return;
            }

            Subscription.TryAckOrNack(basicDeliverEventArgs.ConsumerTag, true, subscriptionInfo.Model, basicDeliverEventArgs.DeliveryTag, false, false, _watcher);
        }

        protected volatile ConsumerStatus _status = ConsumerStatus.Active;

        /// <summary>
        /// Determine whether the object has been disposed
        /// </summary>
        public ConsumerStatus Status => _status;

        protected bool IsDisposed => _status == ConsumerStatus.Disposed;

        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            _status = ConsumerStatus.Disposing;

            //NOTE: Wait all current running tasks to finish and after that dispose the objects
            DateTime timeOut = DateTime.Now.AddSeconds(Global.ConsumerDisposeTimeoutInSeconds);

            while (MessageInProgressCount() > 0 && DateTime.Now <= timeOut)
            {
                _watcher.InfoFormat("Wait for {0} messages in progress", MessageInProgressCount());
                Thread.Sleep(1000);
            }
            _status = ConsumerStatus.Disposed;
            _pool.Dispose();
            CloseQueue();
        }

        private int MessageInProgressCount()
        {
            lock (Subscription.OutstandingDeliveryTags)
            {
                return Subscription.OutstandingDeliveryTags.ContainsKey(ConsumerTag)
                                    ? Subscription.OutstandingDeliveryTags[ConsumerTag].Count
                                    : 0;
            }
        }
    }
}
