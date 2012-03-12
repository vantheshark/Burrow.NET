using System;

namespace Burrow
{
    public interface ITunnel
    {
        event Action OnOpened;

        event Action OnClosed;

        bool IsOpenning { get; }

        void Publish<T>(T rabbit);

        void Publish<T>(T rabbit, string routingKey);
        
        /// <summary>
        /// SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="onReceiveMessage"></param>
        void Subscribe<T>(string subscriptionName, Action<T> onReceiveMessage);

        /// <summary>
        /// SubscriptionName together with the type of Message can be used to define the queue name in IRouteFinder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <param name="onReceiveMessage"></param>
        void SubscribeAsync<T>(string subscriptionName, Action<T> onReceiveMessage);

        void SetRouteFinder(IRouteFinder routeFinder);

        void SetSerializer(ISerializer serializer);

        void SetPersistentMode(bool persistentMode);
    }
}
