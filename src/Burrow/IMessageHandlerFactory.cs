using System;

namespace Burrow
{
    /// <summary>
    /// Responsible for creating <see cref="IMessageHandler"/>
    /// </summary>
    public interface IMessageHandlerFactory : IDisposable
    {
        /// <summary>
        /// Create a <see cref="IMessageHandler"/> using providied subscriptionName and a delegate to handle the message & MessageDeliverEventArgs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="msgHandlingAction"></param>
        /// <returns></returns>
        IMessageHandler Create<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> msgHandlingAction);
    }
}