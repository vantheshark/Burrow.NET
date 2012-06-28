using System;
using RabbitMQ.Client.Events;

namespace Burrow
{
    public interface IConsumerErrorHandler : IDisposable
    {
        /// <summary>
        /// Provide an error handling strategy when there is an error processing the message
        /// </summary>
        /// <param name="deliverEventArgs"></param>
        /// <param name="exception"></param>
        void HandleError(BasicDeliverEventArgs deliverEventArgs, Exception exception);
    }
}
