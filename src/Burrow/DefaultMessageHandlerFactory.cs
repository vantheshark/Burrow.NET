using System;

namespace Burrow
{
    public class DefaultMessageHandlerFactory : IMessageHandlerFactory
    {
        protected readonly IConsumerErrorHandler _consumerErrorHandler;
        protected readonly IRabbitWatcher _watcher;
        protected readonly ISerializer _messageSerializer;

        public DefaultMessageHandlerFactory(IConsumerErrorHandler consumerErrorHandler, ISerializer messageSerializer, IRabbitWatcher watcher)
        {
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

            _consumerErrorHandler = consumerErrorHandler;
            _messageSerializer = messageSerializer;
            _watcher = watcher;
        }
        
        public virtual IMessageHandler Create<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> msgHandlingAction)
        {
            return new DefaultMessageHandler<T>(subscriptionName, msgHandlingAction, _consumerErrorHandler, _messageSerializer, _watcher);
        }

        public void Dispose()
        {
            _consumerErrorHandler.Dispose();
        }
    }
}