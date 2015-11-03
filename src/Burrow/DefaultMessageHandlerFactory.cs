using System;
using System.Diagnostics.CodeAnalysis;

namespace Burrow
{
    public class DefaultMessageHandlerFactory : IMessageHandlerFactory, IObserver<ISerializer>
    {
        protected readonly IConsumerErrorHandler _consumerErrorHandler;
        protected readonly IRabbitWatcher _watcher;
        protected ISerializer _messageSerializer;

        public DefaultMessageHandlerFactory(IConsumerErrorHandler consumerErrorHandler, ISerializer messageSerializer, IRabbitWatcher watcher)
        {
            if (consumerErrorHandler == null)
            {
                throw new ArgumentNullException(nameof(consumerErrorHandler));
            }

            if (messageSerializer == null)
            {
                throw new ArgumentNullException(nameof(messageSerializer));
            }

            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
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

        [ExcludeFromCodeCoverage]
        public void OnNext(ISerializer value)
        {
            _messageSerializer = value;
        }

        [ExcludeFromCodeCoverage]
        public void OnError(Exception error)
        {
        }

        [ExcludeFromCodeCoverage]
        public void OnCompleted()
        {
        }
    }
}