using System;

namespace Burrow
{
    public interface IMessageHandlerFactory : IDisposable
    {
        IMessageHandler Create<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> msgHandlingAction);
    }
}