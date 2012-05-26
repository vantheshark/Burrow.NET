using System;

namespace Burrow.Extras.Internal
{
    public interface ITunnelWithPriorityQueuesSupport : ITunnel
    {
        void Publish<T>(T rabbit, int priority);

        void Publish<T>(T rabbit, string routingKey, int priority);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be automatically acked once the callback executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="priority"> </param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        void Subscribe<T>(string subscriptionName, int priority, Action<T> onReceiveMessage);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be not automatically acked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <returns>Subscription object which can be used to send Ack or NoAck message to server by the delivery tag received in the callback</returns>
        Subscription Subscribe<T>(string subscriptionName, int priority, Action<T, MessageDeliverEventArgs> onReceiveMessage);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be automatically acked once the callback executed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="priority"> </param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        void SubscribeAsync<T>(string subscriptionName, int priority, Action<T> onReceiveMessage);

        /// <summary>
        /// Subscribe to queue by using subscriptionName, the message will be not automatically acked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName">SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder</param>
        /// <param name="priority"> </param>
        /// <param name="onReceiveMessage">A callback method to process received message</param>
        /// <returns>Subscription object which can be used to send Ack or NoAck message to server by the delivery tag received in the callback</returns>
        Subscription SubscribeAsync<T>(string subscriptionName, int priority, Action<T, MessageDeliverEventArgs> onReceiveMessage);
    }
}
