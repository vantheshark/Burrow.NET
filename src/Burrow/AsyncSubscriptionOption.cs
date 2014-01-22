using System;

namespace Burrow
{
    /// <summary>
    /// Provide options to asynchronously subscribe to a queue, the messages will not be automatically acked
    /// <para>You have to use the returned <see cref="Subscription"/> object to ack/nack the message when finish</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncSubscriptionOption<T>
    {
        /// <summary>
        /// SubscriptionName together with the type of Message can be used to define the queue name with IRouteFinder
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// The number of threads to process messaages, default is 1
        /// </summary>
        public ushort BatchSize { get; set; }

        /// <summary>
        /// Specify the message prefetch size for the subscribed queue
        /// </summary>
        public uint QueuePrefetchSize { get; set; }

        /// <summary>
        /// Optional, if not set the RouteFinder from the Tunnel object will be used
        /// </summary>
        public IRouteFinder RouteFinder { get; set; }

        /// <summary>
        /// A callback method to process received message
        /// </summary>
        public Action<T, MessageDeliverEventArgs> MessageHandler { get; set; }
    }
}