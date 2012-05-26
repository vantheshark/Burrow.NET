using System.Diagnostics;

namespace Burrow.Extras
{
    [DebuggerStepThrough]
    public class QueueSetupData
    {
        /// <summary>
        /// How long a message published to a queue can live before it is discarded (milliseconds).(Sets the "x-message-ttl" argument.)
        /// </summary>
        public int MessageTimeToLive { get; set; }

        public bool Durable { get; set; }

        /// <summary>
        /// If yes, the queue will delete itself after at least one consumer has connected, and then all consumers have disconnected.
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// How long a queue can be unused for before it is automatically deleted (milliseconds). (Sets the "x-expires" argument.) 
        /// </summary>
        public int AutoExpire { get; set; }

        public string SubscriptionName { get; set; }

        /// <summary>
        /// AKA Topic
        /// </summary>
        public string RoutingKey { get; set; }

        public QueueSetupData()
        {
            Durable = true;
        }
    }
}