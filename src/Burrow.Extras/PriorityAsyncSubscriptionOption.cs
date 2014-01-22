using System;
using Burrow.Extras.Internal;

namespace Burrow.Extras
{
    /// <summary>
    /// Provide options to asynchronously subscribe to priority queues, the messages will not be automatically acked
    /// <para>You have to use the returned <see cref="CompositeSubscription"/> object to ack/nack the message when finish</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityAsyncSubscriptionOption<T> : AsyncSubscriptionOption<T>, IPrioritySubscriptionOption
    {
        /// <summary>
        /// The max priority level value for priority queues
        /// <para>If you have Queue_Priority0, Queue_Priority1, Queue_Priority2 then MaxPriorityLevel = 2</para>
        /// </summary>
        public uint MaxPriorityLevel { get; set; }

        /// <summary>
        /// Optional, a type of IComparer`[T] to compare the messages priorities
        /// </summary>
        public Type ComparerType { get; set; }

        /// <summary>
        /// Optional, provide a class to resolve the suffix name for priority queues, default is _Priority[PriorityNumber]
        /// </summary>
        public IPriorityQueueSuffix QueueSuffixNameConvention { get; set; }

        /// <summary>
        /// Optional, provide a way to select different prefetch size for different priority queues. Default is <see cref="AsyncSubscriptionOption{T}.QueuePrefetchSize"/>
        /// </summary>
        public Func<uint, uint> QueuePrefetchSizeSelector { get; set; }
    }
}