using System;
using System.Collections;
using Burrow.Extras.Internal;

namespace Burrow.Extras
{
    /// <summary>
    /// A rabbit tunnel interface with Priority support
    /// </summary>
    public interface ITunnelWithPrioritySupport : ITunnel
    {
        /// <summary>
        /// Publish a message to a "headers" exchange (with priority support) defined by route finder. The message will eventually be routed to the queue(s) by provided priority value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit">A message to publish</param>
        /// <param name="priority"></param>
        void Publish<T>(T rabbit, uint priority);

        /// <summary>
        /// Publish a message with a routing key and custom headers value to a "headers" exchange (with priority support) defined by route finder. The message will eventually be routed to the queue(s) by provided priority value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit">A message to publish</param>
        /// <param name="priority"></param>
        /// <param name="customHeaders">If you're publishing to a Header exchange, this method allows you to put your header parameters</param>
        void Publish<T>(T rabbit, uint priority, IDictionary customHeaders);

        /// <summary>
        /// Subscribe synchronously to priority queues and auto ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel">n if you have following priority levels: 0 -&gt; n; 0 is the lowest level by default</param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null);

        /// <summary>
        /// Subscribe synchronously to priority queues and not ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel">n if you have following priority levels: 0 -&gt; n; 0 is the lowest level by default</param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        /// <returns>CompositeSubscription is used to ack the messages later using the consumer tag and delivery id from MessageDeliverEventArgs
        /// <para>Because this is async method, you must ensure that the subscription is created before using it to ack/nack messages</para>
        /// </returns>
        CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null);

        /// <summary>
        /// Subscribe asynchronously to priority queues and auto ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel">n if you have following priority levels: 0 -&gt; n; 0 is the lowest level by default</param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        /// <param name="batchSize">The number of threads to process messages, Default is Global.DefaultConsumerBatchSize</param>
        CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage, Type comparerType = null, ushort? batchSize = null);

        /// <summary>
        /// Subscribe asynchronously to priority queues and not ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel">n if you have following priority levels: 0 -&gt; n; 0 is the lowest level by default</param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        /// <param name="batchSize">The number of threads to process messages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns>CompositeSubscription is used to ack the messages later using the consumer tag and delivery id from MessageDeliverEventArgs
        /// <para>Because this is async method, you must ensure that the subscription is created before using it to ack/nack messages</para>
        /// </returns>
        CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null, ushort? batchSize = null);


        /// <summary>
        /// Return message count of all priority queues defined by maxPriorityLevel and the combination of message T's type and its subscriptionName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel">n if you have following priority levels: 0 -&gt; n; 0 is the lowest level by default</param>
        /// <returns></returns>
        uint GetMessageCount<T>(string subscriptionName, uint maxPriorityLevel);
    }
}
