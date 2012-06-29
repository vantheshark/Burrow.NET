using System;
using RabbitMQ.Client;

namespace Burrow
{
    public interface IConsumerManager : IDisposable
    {
        IMessageHandlerFactory MessageHandlerFactory { get; }

        /// <summary>
        /// Create a synchronous IBasicConsumer, this consumer should ack the message after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="consumerTag"></param>
        /// <param name="onReceiveMessage"></param>
        /// <returns></returns>
        IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage);

        /// <summary>
        /// Create a synchronous IBasicConsumer this consumer should not ack the messages after handling it.
        /// In fact, the system should act the messages later based on the information provided in MessageDeliverEventArgs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="consumerTag"></param>
        /// <param name="onReceiveMessage"></param>
        /// <returns></returns>
        IBasicConsumer CreateConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T, MessageDeliverEventArgs> onReceiveMessage);

        /// <summary>
        /// Create a asynchronous IBasicConsumer which can start a number of batchSize threads to consume the queue, this consumer should ack the messages after handling them
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="consumerTag"></param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="batchSize">The number of threads to process messaages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns></returns>
        IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T> onReceiveMessage, ushort? batchSize = null);


        /// <summary>
        /// Create a asynchronous IBasicConsumer which can start a number of batchSize threads to consume the queue, this consumer should not ack the messages after handling them.
        /// In fact, the system should act the messages later based on the information provided in MessageDeliverEventArgs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="consumerTag"></param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="batchSize">The number of threads to process messaages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns></returns>
        IBasicConsumer CreateAsyncConsumer<T>(IModel channel, string subscriptionName, string consumerTag, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? batchSize = null);

        /// <summary>
        /// Dispose/clear all created consumer once the connection to RabbitMQ server is dropped
        /// </summary>
        void ClearConsumers();
    }
}
