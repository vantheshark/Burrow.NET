using System.Collections.Generic;

namespace Burrow.Extras
{
    /// <summary>
    /// A DTO object which contains all required data to create Exchange, Queue if they do not exist
    /// </summary>
    public class RouteSetupData
    {
        /// <summary>
        /// A route finder object which is used to resolve exchange name and queue name to create
        /// </summary>
        public IRouteFinder RouteFinder { get; set; }
        
        /// <summary>
        /// Exchange setup data
        /// </summary>
        public ExchangeSetupData ExchangeSetupData { get; set; }
        
        /// <summary>
        /// Queue setup data
        /// </summary>
        public QueueSetupData QueueSetupData { get; set; }
        
        /// <summary>
        /// The name of subscriber who will use this route (Exchange & Queue)
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Optional arguments to bind the queue to the exchange
        /// </summary>
        public IDictionary<string, object> OptionalBindingData { get; private set; }

        /// <summary>
        /// Initialize a RouteSetupData
        /// </summary>
        public RouteSetupData()
        {
            OptionalBindingData = new Dictionary<string, object>();
        }
    }
}