using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Burrow
{
    /// <summary>
    /// Implement this interface to provide methods to subscribe & publish messages
    /// </summary>
    public interface ITunnel : IDisposable
    {
        /// <summary>
        /// This event will be fired once a connection to server is established
        /// </summary>
        event Action OnOpened;

        /// <summary>
        /// A event to be fired when the tunnel is closed, at this point any activities such as ack/nack can't be used because there is no connection
        /// </summary>
        event Action OnClosed;

        /// <summary>
        /// This event will be fired once a consumer is disconnected, for example you ack a msg with wrong delivery id (I blame RabbitMQ.Client guys)
        /// </summary>
        event Action<Subscription> ConsumerDisconnected;

        /// <summary>
        /// Determine whether a connection is open
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// Public access to the channel object which is used for publishing messages
        /// <para>for those who wish to use Confirms (aka Publisher Acknowledgements) can confirmSelect() or even txSelect() the channel</para>
        /// <para>Reference: http://www.rabbitmq.com/confirms.html </para>
        /// <para>Reference: http://www.rabbitmq.com/blog/2011/02/10/introducing-publisher-confirms/ </para>
        /// </summary>
        IModel DedicatedPublishingChannel { get; }

        /// <summary>
        /// Publish a message to an exchange defined by route finder. The message will eventually be routed to the queue(s) by the routing key resolved by route finder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit">A message to publish</param>
        void Publish<T>(T rabbit);

        /// <summary>
        /// Publish a message with a routing key to an exchange defined by route finder. The message will eventually be routed to the queue(s) by the routing key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit">A message to publish</param>
        /// <param name="routingKey"></param>
        void Publish<T>(T rabbit, string routingKey);

		/// <summary>
        /// Use this method to publish a message with custome headers to a "headers" exchange defined by route finder. 
        /// <para>The message will be published to that exchange with a routing key resolved by the route finder</para>
        /// <para>So if the target exchange is Direct or Fanout, the customHeaders have no value here and should be ignored by RabbitMQ. </para>
        /// <para>This method should be used when you're sure that you're publishing to a "headers" exchange</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit">A message to publish</param>
        /// <param name="customHeaders">If you're publishing to a Header exchange, this method allows you to put your header parameters</param>
        void Publish<T>(T rabbit, IDictionary<string, object> customHeaders);

        /// <summary>
        /// Subscribe to queue with provided option, the message WILL be automatically acked after the callback executed
        /// <para>You DON'T have to use the returned <see cref="Subscription"/> object to ack/nack because it's done automatically for you</para>
        /// </summary>
        Subscription Subscribe<T>(SubscriptionOption<T> subscriptionOption);

        /// <summary>
        /// Subscribe to queue with provided option, the message WON'T be automatically acked after the callback executed
        /// <para>You have to use the returned <see cref="Subscription"/> object to ack/nack the message when finish</para>
        /// </summary>
        Subscription SubscribeAsync<T>(AsyncSubscriptionOption<T> subscriptionOption);
        
        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be automatically acked once the callback executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        [Obsolete("Use Subscribe with SubscriptionOption instead")]
        Subscription Subscribe<T>(string subscriptionName, Action<T> onReceiveMessage);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be not automatically acked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <returns>Subscription object which can be used to send Ack or NoAck message to server by the delivery tag received in the callback
		/// <para>Because this is async method, you must ensure that the subscription is created before using it to ack/nack messages</para>
		/// </returns>
        [Obsolete("Use SubscribeAsync with AsyncSubscriptionOption instead")]
        Subscription Subscribe<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be automatically acked once the callback executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <param name="batchSize">The number of threads to process messages, Default is Global.DefaultConsumerBatchSize</param>
		[Obsolete("Use Subscribe with SubscriptionOption instead")]
        Subscription SubscribeAsync<T>(string subscriptionName, Action<T> onReceiveMessage, ushort? batchSize = null);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message won't be automatically acked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <param name="batchSize">The number of threads to process messages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns>Subscription object which can be used to send Ack or NoAck message to server by the delivery tag received in the callback
        /// <para>Because this is async method, you must ensure that the subscription is created before using it to ack/nack messages</para>
        /// </returns>
        [Obsolete("Use SubscribeAsync with AsyncSubscriptionOption instead")]
        Subscription SubscribeAsync<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? batchSize = null);

        /// <summary>
        /// Change the route finder of current tunnel
        /// <para>This routeFinder will be used when publishing messages</para>
        /// <para>This routeFinder will also be used when subscribe to queue if the SubscriptionOption doesn't have RouteFinder set</para>
        /// </summary>
        /// <param name="routeFinder"></param>
        void SetRouteFinder(IRouteFinder routeFinder);

        /// <summary>
        /// Change serializer of current tunnel
        /// </summary>
        /// <param name="serializer"></param>
        void SetSerializer(ISerializer serializer);

        /// <summary>
        /// Change persisten mode for published messages
        /// </summary>
        /// <param name="persistentMode"></param>
        void SetPersistentMode(bool persistentMode);

        /// <summary>
        /// Return message count of a current queue whose name is determined by type of message T and the subscriptionName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionOption"> </param>
        /// <returns></returns>
        uint GetMessageCount<T>(SubscriptionOption<T> subscriptionOption);

        /// <summary>
        /// Return message count of provided queueName
        /// </summary>
        uint GetMessageCount(string queueName);
    }
}
