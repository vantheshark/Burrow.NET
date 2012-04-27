namespace Burrow.Extras
{
    public class QueueSetupData
    {
        /// How long a message published to a queue can live before it is discarded (milliseconds).(Sets the "x-message-ttl" argument.)
        public int MessageTimeToLive { get; set; }

        public bool Durable { get; set; }

        /// If yes, the queue will delete itself after at least one consumer has connected, and then all consumers have disconnected.
        public bool AutoDelete { get; set; }

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