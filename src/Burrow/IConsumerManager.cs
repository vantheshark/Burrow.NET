using System;
using RabbitMQ.Client;

namespace Burrow
{
    /// <summary>
    /// Responsible for creating and tracking created <see cref="IBasicConsumer"/>
    /// </summary>
    public interface IConsumerManager : IDisposable
    {
        /// <summary>
        /// A public access to <see cref="IMessageHandlerFactory"/>
        /// </summary>
        IMessageHandlerFactory MessageHandlerFactory { get; }

        /// <summary>
        /// Create a asynchronous IBasicConsumer which can spawn 1 or more threads to consume messages the queue, this consumer should ack the messages after handling them
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="consumerThreadCount">The number of threads to process messaages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns></returns>
        IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, Action<T> onReceiveMessage, ushort? consumerThreadCount = null);

        /// <summary>
        /// Create a asynchronous IBasicConsumer which can spawn 1 or more threads to consume messages from the queue, this consumer should NOT ack the messages after handling them.
        /// <para>Indeed, the system should act the messages later based on the data provided in MessageDeliverEventArgs</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="consumerThreadCount">The number of threads to process messaages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns></returns>
        IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? consumerThreadCount = null);

        /// <summary>
        /// Dispose/clear all created consumer once the connection to RabbitMQ server is dropped
        /// </summary>
        void ClearConsumers();
    }
}
