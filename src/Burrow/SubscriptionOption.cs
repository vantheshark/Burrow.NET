using System;

namespace Burrow
{
    /// <summary>
    /// Provide options to subscribe to a queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SubscriptionOption<T>
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
        public Action<T> MessageHandler { get; set; }
    }
}