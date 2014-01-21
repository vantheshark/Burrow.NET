using System.Collections.Generic;
using System.Diagnostics;

namespace Burrow.Extras
{
    /// <summary>
    /// A DTO object which contains information to create a queue
    /// </summary>
    [DebuggerStepThrough]
    public class QueueSetupData
    {
        /// <summary>
        /// How long a message published to a queue can live before it is discarded (milliseconds).(Sets the "x-message-ttl" argument.)
        /// </summary>
        public int MessageTimeToLive { get; set; }

        /// <summary>
        /// Durable queues are persisted to disk and thus survive broker restarts. Queues that are not durable are called transient. Not all scenarios and use cases mandate queues to be durable.
        ///Durability of a queue does not make messages that are routed to that queue durable. If broker is taken down and then brought back up, durable queue will be re-declared during broker startup, however, only persistent messages will be recovered.
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// If yes, the queue will delete itself after at least one consumer has connected, and then all consumers have disconnected.
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// How long a queue can be unused for before it is automatically deleted (milliseconds). (Sets the "x-expires" argument.) 
        /// </summary>
        public int AutoExpire { get; set; }

        /// <summary>
        /// http://www.rabbitmq.com/dlx.html
        /// </summary>
        public string DeadLetterExchange { get; set; }

        /// <summary>
        /// You may also specify a routing key to be used when dead-lettering messages. If this is not set, the message's own routing keys will be used.
        /// http://www.rabbitmq.com/dlx.html
        /// </summary>
        public string DeadLetterRoutingKey { get; set; }

        /// <summary>
        /// Optional arguments when create queue
        /// </summary>
        public IDictionary<string, object> Arguments { get; private set; }

        /// <summary>
        /// Initialize a QueueSetupData
        /// </summary>
        public QueueSetupData()
        {
            Durable = true;
            Arguments = new Dictionary<string, object>();
        }
    }
}