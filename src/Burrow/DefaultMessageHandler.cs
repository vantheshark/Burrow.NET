using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow
{
    public class DefaultMessageHandler : IMessageHandler
    {
        protected readonly IRabbitWatcher _watcher;
        protected readonly Func<BasicDeliverEventArgs, Task> _jobFactory;
        protected readonly IConsumerErrorHandler _consumerErrorHandler;

        public event MessageHandlingEvent HandlingComplete;

        public DefaultMessageHandler(IConsumerErrorHandler consumerErrorHandler,
                                     Func<BasicDeliverEventArgs, Task> jobFactory,
                                     IRabbitWatcher watcher)
        {
            if (consumerErrorHandler == null)
            {
                throw new ArgumentNullException("consumerErrorHandler");
            }
            if (jobFactory == null)
            {
                throw new ArgumentNullException("jobFactory");
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }

            _watcher = watcher;
            _consumerErrorHandler = consumerErrorHandler;
            _jobFactory = jobFactory;
        }

        public virtual void BeforeHandlingMessage(IBasicConsumer consumer, BasicDeliverEventArgs eventArg)
        {
        }

        public virtual void AfterHandlingMessage(IBasicConsumer consumer, BasicDeliverEventArgs eventArg)
        {
        }

        public virtual void HandleError(IBasicConsumer consumer, BasicDeliverEventArgs eventArg, Exception exception)
        {
            _watcher.ErrorFormat(BuildErrorLogMessage(eventArg, exception));
            _consumerErrorHandler.HandleError(eventArg, exception);
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

        public virtual void HandleMessage(IBasicConsumer consumer, BasicDeliverEventArgs eventArg)
        {
            var completionTask = _jobFactory(eventArg);
            completionTask.ContinueWith(task =>
            {
                try
                {
                    if (task.IsFaulted)
                    {
                        HandleError(consumer, eventArg, task.Exception);
                    }
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }
                finally
                {
                    // Broadcast msgs
                    AfterHandlingMessage(consumer, eventArg);

                    //NOTE: Only this way, the new event to interupt other consumers will override previous resume event
                    if (HandlingComplete != null)
                    {
                        // Release pool + DoAck
                        HandlingComplete(eventArg);
                    }
                }
            });
        }
    }
}