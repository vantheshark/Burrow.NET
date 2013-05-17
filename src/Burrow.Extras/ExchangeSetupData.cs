using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Burrow.Extras
{
    /// <summary>
    /// DTO object that contains information to create an RabbitMQ exchange
    /// </summary>
    [DebuggerStepThrough]
    public class ExchangeSetupData
    {
        /// <summary>
        /// Direct, Fanout, Topic, Headers
        /// </summary>
        public string ExchangeType { get; set; }
        
        /// <summary>
        /// Exchanges can be durable or transient. Durable exchanges survive broker restart whereas transient exchanges do not (they have to be redeclared when broker comes back online). Not all scenarios and use cases require exchanges to be durable.
        /// </summary>
        public bool Durable { get; set; }
        /// <summary>
        /// If yes, the exchange will delete itself after at least one queue or exchange has been bound to this one, and then all queues or exchanges have been unbound. 
        /// </summary>
        public bool AutoDelete { get; set; }

        /// <summary>
        /// Optional arguments when create exchange
        /// </summary>
        public IDictionary Arguments { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ExchangeSetupData()
        {
            Durable = true;
            ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
            Arguments = new Dictionary<string, object>();
        }
    }
}
