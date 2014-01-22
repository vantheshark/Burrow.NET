using System;
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
        /// This event will be fired once a connection to server is lost
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
        /// <para>Reference: http://www.rabbitmq.com/confirms.html</para>
        /// <para>Reference: http://www.rabbitmq.com/blog/2011/02/10/introducing-publisher-confirms/</para>
        /// </summary>
        IModel DedicatedPublishingChannel { get; }

        /// <summary>
        /// Publish a message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit"></param>
        void Publish<T>(T rabbit);

        /// <summary>
        /// Publish a message using routing key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rabbit"></param>
        /// <param name="routingKey"></param>
        void Publish<T>(T rabbit, string routingKey);

        /// <summary>
        /// Subscribe to queue with provided option, the message WILL be automatically acked after the callback executed
        /// </summary>
        void Subscribe<T>(SubscriptionOption<T> subscriptionOption);

        /// <summary>
        /// Subscribe to queue with provided option, the message WON'T be automatically acked after the callback executed
        /// <para>You have to use the returned <see cref="Subscription"/> object to ack/nack the message when finish</para>
        /// </summary>
        void SubscribeAsync<T>(AsyncSubscriptionOption<T> subscriptionOption);
        
        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be automatically acked once the callback executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        [Obsolete("Use Subscribe with SubscriptionOption instead")]
        void Subscribe<T>(string subscriptionName, Action<T> onReceiveMessage);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be not automatically acked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <returns>Subscription object which can be used to send Ack or NoAck message to server by the delivery tag received in the callback</returns>
        [Obsolete("Use Subscribe with AsyncSubscriptionOption instead")]
        Subscription Subscribe<T>(string subscriptionName, Action<T, MessageDeliverEventArgs> onReceiveMessage);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be automatically acked once the callback executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <param name="batchSize">The number of threads to process messaages, Default is Global.DefaultConsumerBatchSize</param>
        [Obsolete("Use Subscribe with SubscriptionOption instead")]
        void SubscribeAsync<T>(string subscriptionName, Action<T> onReceiveMessage, ushort? batchSize = null);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message won't be automatically acked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <param name="batchSize">The number of threads to process messaages, Default is Global.DefaultConsumerBatchSize</param>
        /// <returns>Subscription object which can be used to send Ack or NoAck message to server by the delivery tag received in the callback</returns>
        [Obsolete("Use Subscribe with AsyncSubscriptionOption instead")]
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
        /// Change persisten mode for published messages
        /// </summary>
        /// <param name="persistentMode"></param>
        void SetPersistentMode(bool persistentMode);

        /// <summary>
        /// Return message count of a current queue whose name is determined by type of message T and the subscriptionName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <returns></returns>
        uint GetMessageCount<T>(string subscriptionName);

        /// <summary>
        /// Return message count of provided queueName
        /// </summary>
        uint GetMessageCount(string queueName);
    }
}
