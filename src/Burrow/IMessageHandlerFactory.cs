using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Burrow
{
    public interface IMessageHandlerFactory : IDisposable
    {
        IMessageHandler Create(Func<BasicDeliverEventArgs, Task> jobFactory);
    }
}