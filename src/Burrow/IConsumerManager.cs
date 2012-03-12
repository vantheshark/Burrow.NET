using System;
using RabbitMQ.Client;

namespace Burrow
{
    public interface IConsumerManager : IDisposable
    {
        IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage);
        IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage);
        void ClearConsumers();
        int BatchSize { get; }
    }
}
