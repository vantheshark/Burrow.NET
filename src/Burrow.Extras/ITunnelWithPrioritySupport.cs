using System;
using Burrow.Extras.Internal;

namespace Burrow.Extras
{
    public interface ITunnelWithPrioritySupport : ITunnel
    {
        void Publish<T>(T rabbit, uint priority);
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
    }
}
