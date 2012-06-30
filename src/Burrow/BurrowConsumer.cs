using System;
using System.IO;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Util;

namespace Burrow
{
    public class BurrowConsumer : QueueingBasicConsumer, IDisposable
    {
        protected readonly IRabbitWatcher _watcher;
        private readonly bool _autoAck;
        

        private readonly object _sharedQueueLock = new object();
        private readonly Thread _subscriptionCallbackThread;
        protected Semaphore _pool { get; private set; }
        private readonly IMessageHandler _messageHandler;

        public BurrowConsumer(IModel channel,
                              IMessageHandler messageHandler,
                              IRabbitWatcher watcher,                     
                              
                              bool autoAck,
                              int batchSize)
            : base(channel, new SharedQueue())
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
                throw new ArgumentNullException("batchSize", "batchSize must be greater than or equal 1");
            }

            Model.ModelShutdown += WhenChannelShutdown;
            Model.BasicRecoverAsync(true);
            BatchSize = batchSize;

            _pool = new Semaphore(BatchSize, BatchSize);
            _watcher = watcher;
            _autoAck = autoAck;


            _messageHandler = messageHandler;
            _messageHandler.HandlingComplete += MessageHandlerHandlingComplete;
            _subscriptionCallbackThread = new Thread(_ =>
            {
                Thread.CurrentThread.Name = string.Format("Consumer thread: {0}", ConsumerTag);
                while (!_disposed)
                {
                    try
                    {
                        BasicDeliverEventArgs deliverEventArgs;
                        lock (_sharedQueueLock)
                        {
                            _pool.WaitOne();
                            deliverEventArgs = (BasicDeliverEventArgs) Queue.Dequeue();
                        }
                        if (deliverEventArgs != null)
                        {
                            _messageHandler.BeforeHandlingMessage(this, deliverEventArgs);
                            HandleMessageDelivery(deliverEventArgs);
                        }
                    }
                    catch(ThreadAbortException)
                    {
                        _watcher.WarnFormat("The consumer thread {0} is aborted", ConsumerTag);
                    }
                    catch (EndOfStreamException)
                    {
                        // do nothing here, EOS fired when queue is closed
                        // Looks like the connection has gone away, so wait a little while
                        // before continuing to poll the queue
                        Thread.Sleep(10);
                    }
                }
            });
            _subscriptionCallbackThread.Start();
        }

        private void MessageHandlerHandlingComplete(BasicDeliverEventArgs eventArgs)
        {
            _pool.Release();
            if (_autoAck)
            {
                DoAck(eventArgs, this);
            }
        }

        protected virtual void WhenChannelShutdown(IModel model, ShutdownEventArgs reason)
        {
            Queue.Close();
            _subscriptionCallbackThread.Abort();
        }

        private void HandleMessageDelivery(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                _watcher.DebugFormat("Received CId: {0}, RKey: {1}, DTag: {2}", basicDeliverEventArgs.BasicProperties.CorrelationId, basicDeliverEventArgs.RoutingKey, basicDeliverEventArgs.DeliveryTag);
                _messageHandler.HandleMessage(this, basicDeliverEventArgs);
            }
            catch (Exception exception)
            {
                _messageHandler.HandleError(this, basicDeliverEventArgs, exception);
                if (_autoAck)
                {
                    DoAck(basicDeliverEventArgs, this);
                }
                _pool.Release();
            }
        }

        protected virtual void DoAck(BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
        {
            if (_disposed || !subscriptionInfo.Model.IsOpen)
            {
                return;
            }

            const string failedToAckMessage = "Basic ack failed because chanel was closed with message {0}. " +
                                              "Message remains on RabbitMQ and will be retried.";

            try
            {
                subscriptionInfo.Model.BasicAck(basicDeliverEventArgs.DeliveryTag, false);
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

        public int BatchSize { get; private set; }

        private volatile bool _disposed;
        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _pool.Dispose();
            Queue.Close();
            _disposed = true;
        }
    }
}
