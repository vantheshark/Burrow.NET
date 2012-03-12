using System;
using RabbitMQ.Client;

namespace Burrow
{
    public interface IDurableConnection : IDisposable
    {
        event Action Connected;
        event Action Disconnected;
        bool IsConnected { get; }
        ConnectionFactory ConnectionFactory { get; }
        IModel CreateChannel();
    }
}
