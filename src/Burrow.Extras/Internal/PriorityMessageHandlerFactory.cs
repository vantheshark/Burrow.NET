using System;

namespace Burrow.Extras.Internal
{
    internal class PriorityMessageHandlerFactory : DefaultMessageHandlerFactory
    {
        public PriorityMessageHandlerFactory(IConsumerErrorHandler consumerErrorHandler, ISerializer messageSerializer, IRabbitWatcher watcher) 
            : base(consumerErrorHandler, messageSerializer, watcher)
        {
        }

        public override IMessageHandler Create<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> msgHandlingAction)
        {
            return new PriorityMessageHandler<T>(subscriptionName, msgHandlingAction, _consumerErrorHandler, _messageSerializer, _watcher);
        } 
    }
}