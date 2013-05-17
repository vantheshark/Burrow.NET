using System;
using Burrow.Extras.Internal;

namespace Burrow.Extras
{
    /// <summary>
    /// A rabbit tunnel interface with Priority support
    /// </summary>
    public interface ITunnelWithPrioritySupport : ITunnel
    {
        /// <summary>
        /// Publish a message with priority
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit"></param>
        /// <param name="priority"></param>
        void Publish<T>(T rabbit, uint priority);

        /// <summary>
        /// Publish a message with priority and routing key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit"></param>
        /// <param name="routingKey"></param>
        /// <param name="priority"></param>
        void Publish<T>(T rabbit, string routingKey, uint priority);

        /// <summary>
        /// Subscribe synchronously to priority queues and auto ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel"></param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        void Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null);

        /// <summary>
        /// Subscribe synchronously to priority queues and not ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel"></param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        /// <returns>CompositeSubscription is used to ack the messaages later using the consumer tag and delivery id from MessageDeliverEventArgs</returns>
        CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null);

        /// <summary>
        /// Subscribe asynchronously to priority queues and auto ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel"></param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        /// <param name="batchSize">The number of threads to process messaages, Default is Global.DefaultConsumerBatchSize</param>
        void SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null, ushort? batchSize = null);

        /// <summary>
        /// Subscribe asynchronously to priority queues and not ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel"></param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        /// <param name="batchSize">The number of threads to process messaages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns>CompositeSubscription is used to ack the messaages later using the consumer tag and delivery id from MessageDeliverEventArgs</returns>
        CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null, ushort? batchSize = null);


        /// <summary>
        /// Return message count of all priority queues defined by maxPriorityLevel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel"> </param>
        /// <returns></returns>
        uint GetMessageCount<T>(string subscriptionName, uint maxPriorityLevel);
    }
}
