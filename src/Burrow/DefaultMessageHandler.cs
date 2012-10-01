using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow
{
    public class DefaultMessageHandler<T> : IMessageHandler
    {
        protected readonly string _subscriptionName;
        protected readonly IRabbitWatcher _watcher;
        protected readonly Action<T, MessageDeliverEventArgs> _msgHandlingAction;
        protected readonly IConsumerErrorHandler _consumerErrorHandler;
        protected readonly ISerializer _messageSerializer;

        public event MessageHandlingEvent HandlingComplete;
        public event MessageWasNotHandledEvent MessageWasNotHandled;

        public DefaultMessageHandler(string subscriptionName, 
                                     Action<T, MessageDeliverEventArgs> msgHandlingAction,
                                     IConsumerErrorHandler consumerErrorHandler, 
                                     ISerializer messageSerializer, 
                                     IRabbitWatcher watcher)
        {
            if (msgHandlingAction == null)
            {
                throw new ArgumentNullException("msgHandlingAction");
            }

            if (consumerErrorHandler == null)
            {
                throw new ArgumentNullException("consumerErrorHandler");
            }

            if (messageSerializer == null)
            {
                throw new ArgumentNullException("messageSerializer");
            }

            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }

            _subscriptionName = subscriptionName;
            _watcher = watcher;
            _consumerErrorHandler = consumerErrorHandler;
            _messageSerializer = messageSerializer;
            _msgHandlingAction = msgHandlingAction;
        }

        /// <summary>
        /// If you want to do anything before handling the message, speak now or forever hold your peace!
        /// </summary>
        /// <param name="eventArg"></param>
        protected virtual void BeforeHandlingMessage(BasicDeliverEventArgs eventArg)
        {
        }

        /// <summary>
        /// If you want to do anything before handling the message, speak now or forever hold your peace!
        /// </summary>
        /// <param name="eventArg"></param>
        protected virtual void AfterHandlingMessage(BasicDeliverEventArgs eventArg)
        {
        }

        /// <summary>
        /// This method provide the default implementation of error handling.
        /// Infact, it delegates the implementation to ConsumerErrorHandler
        /// </summary>
        /// <param name="eventArg"></param>
        /// <param name="exception"></param>
        public virtual void HandleError(BasicDeliverEventArgs eventArg, Exception exception)
        {
            _watcher.ErrorFormat(BuildErrorLogMessage(eventArg, exception));
            _consumerErrorHandler.HandleError(eventArg, exception);
        }
        
        [ExcludeFromCodeCoverage]
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

        /// <summary>
        /// This method creates a background Task to handle the job.
        /// It will catches all exceptions
        /// </summary>
        /// <param name="eventArgs"></param>
        public void HandleMessage(BasicDeliverEventArgs eventArgs)
        {
            Task.Factory.StartNew(() =>
            {
                bool msgHandled = false;
                try
                {
                    BeforeHandlingMessage(eventArgs);
                    HandleMessage(eventArgs, out msgHandled);
                }
                catch (Exception ex)
                {
#if DEBUG
                    _watcher.ErrorFormat("5. The task to execute the provided callback with DTag: {0} by CTag: {1} has been finished but there is an error", eventArgs.DeliveryTag, eventArgs.ConsumerTag);
#endif
                    _watcher.Error(ex);
                    try
                    {
                        HandleError(eventArgs, ex);
                    }
                    catch (Exception errorHandlingEx)
                    {
                        _watcher.ErrorFormat("Failed to handle the exception: {0} because of {1}", ex.Message, errorHandlingEx.StackTrace);
                    }
                }
                finally
                {
                    CleanUp(eventArgs, msgHandled);
                }
                
            }, Global.DefaultTaskCreationOptionsProvider());
        }

        /// <summary>
        /// This method is actually invoke the callback providded to ITunnel when you subcribe to the queue
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <param name="msgHandled">If message is delivered to client, set to true</param>
        protected virtual void HandleMessage(BasicDeliverEventArgs eventArgs, out bool msgHandled)
        {
            var currentThread = Thread.CurrentThread;
            currentThread.IsBackground = true;
            currentThread.Priority = ThreadPriority.Highest;
#if DEBUG
            _watcher.DebugFormat("4. A task to execute the provided callback with DTag: {0} by CTag: {1} has been started using {2}.",
                                 eventArgs.DeliveryTag,
                                 eventArgs.ConsumerTag,
                                 currentThread.IsThreadPoolThread ? "ThreadPool" : "dedicated Thread");
#endif
            CheckMessageType(eventArgs.BasicProperties);
            var message = _messageSerializer.Deserialize<T>(eventArgs.Body);

            
            _msgHandlingAction(message, new MessageDeliverEventArgs
            {
                ConsumerTag = eventArgs.ConsumerTag,
                DeliveryTag = eventArgs.DeliveryTag,
                SubscriptionName = _subscriptionName,
            });
            msgHandled = true;
#if DEBUG
            _watcher.DebugFormat("5. The task to execute the provided callback with DTag: {0} by CTag: {1} has been finished successfully.",
                                 eventArgs.DeliveryTag,
                                 eventArgs.ConsumerTag);
#endif
        }

        /// <summary>
        /// This method is doing the dirty clean up job: Call the AfterHandlingMessage & fire HandlingComplete event
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <param name="msgHandled"> </param>
        internal void CleanUp(BasicDeliverEventArgs eventArgs, bool msgHandled)
        {
            if (!msgHandled && MessageWasNotHandled != null)
            {
                try
                {
                    MessageWasNotHandled(eventArgs);
                }
                catch (Exception exceptionWhenFiringMessageWasNotDeliveredEvent)
                {
                    _watcher.ErrorFormat("There is an error when trying to fire MessageWasNotDelivered event");
                    _watcher.Error(exceptionWhenFiringMessageWasNotDeliveredEvent);
                }
            }

            try
            {
                AfterHandlingMessage(eventArgs);
            }
            catch (Exception afterHandlingMessageException)
            {
                _watcher.ErrorFormat("There is an error when trying to call AfterHandlingMessage method");
                _watcher.Error(afterHandlingMessageException);
            }

            if (HandlingComplete != null)
            {
                try
                {
                    HandlingComplete(eventArgs);
                }
                catch (Exception exceptionWhenFiringHandlingCompleteEvent)
                {
                    // Properly should Release pool + DoAck on the BurrowConsumer object
                    _watcher.ErrorFormat("There is an error when trying to fire HandlingComplete event");
                    _watcher.Error(exceptionWhenFiringHandlingCompleteEvent);
                }
            }
        }

        protected void CheckMessageType(IBasicProperties properties)
        {
            var typeName = Global.DefaultTypeNameSerializer.Serialize(typeof(T));
            if (properties.Type != typeName)
            {
                _watcher.ErrorFormat("Message type is incorrect. Expected '{0}', but was '{1}'", typeName, properties.Type);
                throw new Exception(string.Format("Message type is incorrect. Expected '{0}', but was '{1}'", typeName, properties.Type));
            }
        }
    }
}