using System;
using System.Collections.Generic;
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
        void Publish<T>(T rabbit, uint priority, IDictionary<string, object> customHeaders);

        /// <summary>
        /// Subscribe to priority queues with provided option, the messages WILL be automatically acked after the callback executed
        /// </summary>
        CompositeSubscription Subscribe<T>(PrioritySubscriptionOption<T> subscriptionOption);

        /// <summary>
        /// Subscribe to queue with provided option, the message WON'T be automatically acked after the callback executed
        /// <para>You have to use the returned <see cref="Subscription"/> object to ack/nack the message when finish</para>
        /// </summary>
        /// <returns>CompositeSubscription is used to ack the messages later using the consumer tag and delivery id from MessageDeliverEventArgs
        /// <para>Because this is async method, you must ensure that the subscription is created before using it to ack/nack messages</para>
        /// </returns>
        CompositeSubscription SubscribeAsync<T>(PriorityAsyncSubscriptionOption<T> subscriptionOption);

        /// <summary>
        /// Subscribe synchronously to priority queues and auto ack the msg after handling it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="maxPriorityLevel">n if you have following priority levels: 0 -&gt; n; 0 is the lowest level by default</param>
        /// <param name="onReceiveMessage"></param>
        /// <param name="comparerType">a type of IComparer`[T] to compare the messages priorities</param>
        [Obsolete("Use Subscribe with PrioritySubscriptionOption instead")]
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
        [Obsolete("Use Subscribe with PrioritySubscriptionOption instead")]
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
        [Obsolete("Use Subscribe with PriorityAsyncSubscriptionOption instead")]
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
		[Obsolete("Use Subscribe with PriorityAsyncSubscriptionOption instead")]
        CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage, Type comparerType = null, ushort? batchSize = null);

        /// <summary>
        /// Return message count of all priority queues
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionOption"> </param>
        /// <returns></returns>
        uint GetMessageCount<T>(PrioritySubscriptionOption<T> subscriptionOption);
    }
}
