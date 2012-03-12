using System;
using RabbitMQ.Client.Events;

namespace Burrow
{
    public interface IConsumerErrorHandler : IDisposable
    {
        void HandleError(BasicDeliverEventArgs deliverEventArgs, Exception exception);
    }
}
