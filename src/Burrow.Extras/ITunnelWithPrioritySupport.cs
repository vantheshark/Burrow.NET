using System;
using Burrow.Extras.Internal;

namespace Burrow.Extras
{
    public interface ITunnelWithPrioritySupport : ITunnel
    {
        void Publish<T>(T rabbit, uint priority);
        void Publish<T>(T rabbit, string routingKey, uint priority);
        void Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage);
        CompositeSubscription Subscribe<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage);
        void SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T> onReceiveMessage);
        CompositeSubscription SubscribeAsync<T>(string subscriptionName, uint maxPriorityLevel, Action<T, MessageDeliverEventArgs> onReceiveMessage);
    }
}
