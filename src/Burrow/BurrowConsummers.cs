using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Util;

namespace Burrow
{
    public abstract class BurrowConsummer : QueueingBasicConsumer, IDisposable
    {
        protected readonly IRabbitWatcher Watcher;
        protected readonly IConsumerErrorHandler ConsumerErrorHandler;
        protected readonly ISerializer Serializer;
        protected readonly Func<BasicDeliverEventArgs, Task> JobFactory;
        
        private readonly object _sharedQueueLock = new object();
        private readonly Thread _subscriptionCallbackThread;
        private readonly Semaphore _pool;

        protected BurrowConsummer(IRabbitWatcher watcher, 
                                  IConsumerErrorHandler consumerErrorHandler, 
                                  ISerializer serializer, 
                                  IModel channel, 
                                  string consumerTag,
                                  Func<BasicDeliverEventArgs, Task> jobFactory,
                                  int batchSize)
            : base(channel, new SharedQueue())
        {
            
            if (consumerErrorHandler == null)
            {
                throw new ArgumentNullException("consumerErrorHandler");
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            if (jobFactory == null)
            {
                throw new ArgumentNullException("jobFactory");
            }
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            if (batchSize < 1)
            {
                throw new ArgumentNullException("batchSize", "batchSize must be greter than or equal 1");
            }
            if (string.IsNullOrEmpty(consumerTag))
            {
                throw new ArgumentNullException("consumerTag", "consumerTag cannot be null or empty");
            }

            Model.ModelShutdown += WhenChannelShutdown;
            Model.BasicRecoverAsync(true);
            BatchSize = batchSize;
            ConsumerTag = consumerTag;
            JobFactory = jobFactory;

            _pool = new Semaphore(BatchSize, BatchSize);
            Watcher = watcher;
            ConsumerErrorHandler = consumerErrorHandler;
            Serializer = serializer;

            _subscriptionCallbackThread = new Thread(_ =>
            {
                while (!_disposed)
                {
                    try
                    {
                        BasicDeliverEventArgs deliverEventArgs;
                        lock (_sharedQueueLock)
                        {
                            _pool.WaitOne();
                            deliverEventArgs = (BasicDeliverEventArgs)Queue.Dequeue();
                        }
                        if (deliverEventArgs != null)
                        {
                            HandleMessageDelivery(deliverEventArgs);
                        }
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

        protected virtual void WhenChannelShutdown(IModel model, ShutdownEventArgs reason)
        {
            Queue.Close();
            _subscriptionCallbackThread.Abort();
        }

        protected virtual void HandleMessageDelivery(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                Watcher.DebugFormat("Subscriber received \t{0}\nCorrelationId \t\t{1}\nDeliveryTag \t\t{2}", basicDeliverEventArgs.RoutingKey, basicDeliverEventArgs.BasicProperties.CorrelationId, basicDeliverEventArgs.DeliveryTag);
                var completionTask = JobFactory(basicDeliverEventArgs);
                completionTask.ContinueWith(task =>
                {
                    try
                    {
                        if (task.IsFaulted)
                        {
                            var exception = task.Exception;
                            Watcher.ErrorFormat(BuildErrorLogMessage(basicDeliverEventArgs, exception));
                            ConsumerErrorHandler.HandleError(basicDeliverEventArgs, exception);
                        }
                        DoAck(basicDeliverEventArgs, this);
                    }
                    catch(Exception ex)
                    {
                        Watcher.Error(ex);
                    }
                    finally
                    {
                        _pool.Release();
                    }
                });
            }
            catch (Exception exception)
            {
                Watcher.ErrorFormat(BuildErrorLogMessage(basicDeliverEventArgs, exception));
                ConsumerErrorHandler.HandleError(basicDeliverEventArgs, exception);
                DoAck(basicDeliverEventArgs, this);
                _pool.Release(); // Just in case there is problem with the Watcher || the completionTask is null
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
                Watcher.WarnFormat(failedToAckMessage, alreadyClosedException.Message);
            }
            catch (IOException ioException)
            {
                Watcher.WarnFormat(failedToAckMessage, ioException.Message);
            }
        }

        protected virtual string BuildErrorLogMessage(BasicDeliverEventArgs basicDeliverEventArgs, Exception exception)
        {
            var message = Encoding.UTF8.GetString(basicDeliverEventArgs.Body);

            var properties = basicDeliverEventArgs.BasicProperties as RabbitMQ.Client.Impl.BasicProperties;
            var propertiesMessage = new StringBuilder();
            if (properties != null)
            {
                properties.AppendPropertyDebugStringTo(propertiesMessage);
            }

            return "Exception thrown by subscription calback.\n" +
                   string.Format("\tExchange:    '{0}'\n", basicDeliverEventArgs.Exchange) +
                   string.Format("\tRouting Key: '{0}'\n", basicDeliverEventArgs.RoutingKey) +
                   string.Format("\tRedelivered: '{0}'\n", basicDeliverEventArgs.Redelivered) +
                   string.Format(" Message:\n{0}\n", message) +
                   string.Format(" BasicProperties:\n{0}\n", propertiesMessage) +
                   string.Format(" Exception:\n{0}\n", exception);
        }

        public int BatchSize { get; private set; }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            Queue.Close();
            _disposed = true;
        }
    }

    public class ParallelConsumer : BurrowConsummer
    {
        private readonly bool _autoAck;

        public ParallelConsumer(IRabbitWatcher watcher, 
                                IConsumerErrorHandler consumerErrorHandler, 
                                ISerializer serializer, 
                                IModel channel, 
                                string consumerTag, 
                                Func<BasicDeliverEventArgs, Task> jobFactory,
                                int batchSize, bool autoAck)
            : base(watcher, consumerErrorHandler, serializer, channel, consumerTag, jobFactory, batchSize)
        {
            _autoAck = autoAck;
        }

        protected override void DoAck(BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
        {
            if (_autoAck)
            {
                base.DoAck(basicDeliverEventArgs, subscriptionInfo);
            }
        }
    }

    public class SequenceConsumer : BurrowConsummer
    {
        private readonly bool _autoAck;

        public SequenceConsumer(IRabbitWatcher watcher,
                                IConsumerErrorHandler consumerErrorHandler,
                                ISerializer serializer,
                                IModel channel,
                                string consumerTag,
                                Func<BasicDeliverEventArgs, Task> jobFactory, bool autoAck)
            : base(watcher, consumerErrorHandler, serializer, channel, consumerTag, jobFactory, 1)
        {
            _autoAck = autoAck;
        }

        protected override void DoAck(BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
        {
            if (_autoAck)
            {
                base.DoAck(basicDeliverEventArgs, subscriptionInfo);
            }
        }
    }
}
