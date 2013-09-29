using System;
using System.Collections;

namespace Burrow
{
    /// <summary>
    /// Use a tunnel object to publish/subscribe
    /// </summary>
    public interface ITunnel : IDisposable
    {
        /// <summary>
        /// A event to be fired when the tunnel is opened
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
        /// <para>So if the target exchange is Direct or Fanout, the customHeaders have no value here and should be ignored by RabbitMQ. 
        /// This method should be used when you're sure that you're publishing to a "headers" exchange</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit">A message to publish</param>
        /// <param name="customHeaders">If you're publishing to a Header exchange, this method allows you to put your header parameters</param>
        void Publish<T>(T rabbit, IDictionary customHeaders);
        
        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be automatically acked once the callback executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
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
        Subscription Subscribe<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be automatically acked once the callback executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <param name="batchSize">The number of threads to process messages, Default is Global.DefaultConsumerBatchSize</param>
        Subscription SubscribeAsync<T>(string subscriptionName, Action<T> onReceiveMessage, ushort? batchSize = null);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be not automatically acked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <param name="batchSize">The number of threads to process messages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns>Subscription object which can be used to send Ack or NoAck message to server by the delivery tag received in the callback
        /// <para>Because this is async method, you must ensure that the subscription is created before using it to ack/nack messages</para>
        /// </returns>
        Subscription SubscribeAsync<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage, ushort? batchSize = null);

        /// <summary>
        /// Change the route finder of current tunnel
        /// </summary>
        /// <param name="routeFinder"></param>
        void SetRouteFinder(IRouteFinder routeFinder);

        /// <summary>
        /// Change serializer of current tunnel
        /// </summary>
        /// <param name="serializer"></param>
        void SetSerializer(ISerializer serializer);

        /// <summary>
        /// Change persisten mode
        /// </summary>
        /// <param name="persistentMode"></param>
        void SetPersistentMode(bool persistentMode);

        /// <summary>
        /// Return message count of a current queue whose name is determined by the combination of message T's type and its subscriptionName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <returns></returns>
        uint GetMessageCount<T>(string subscriptionName);
    }
}
