using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Burrow
{
    public class DefaultMessageHandlerFactory : IMessageHandlerFactory
    {
        protected readonly IConsumerErrorHandler _consumerErrorHandler;
        protected readonly IRabbitWatcher _watcher;

        public DefaultMessageHandlerFactory(IConsumerErrorHandler consumerErrorHandler, IRabbitWatcher watcher)
        {
            if (consumerErrorHandler == null)
            {
                throw new ArgumentNullException("consumerErrorHandler");
            }
            if (watcher == null)
            {
                throw new ArgumentNullException("watcher");
            }

            _consumerErrorHandler = consumerErrorHandler;
            _watcher = watcher;
        }

        public virtual IMessageHandler Create(Func<BasicDeliverEventArgs, Task> jobFactory)
        {
            return new DefaultMessageHandler(_consumerErrorHandler, jobFactory, _watcher);
        }

        public void Dispose()
        {
            _consumerErrorHandler.Dispose();
        }
    }
}