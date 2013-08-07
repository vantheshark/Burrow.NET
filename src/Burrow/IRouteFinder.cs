namespace Burrow
{
    /// <summary>
    /// Implement this interface and set it with the <see cref="ITunnel"/>
    /// </summary>
    public interface IRouteFinder
    {
        /// <summary>
        /// Find the exchange name based on the message type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        string FindExchangeName<T>();
        
        /// <summary>
        /// Find the routing key based on the message type.
        /// </summary>
        /// <typeparam name="T">AKA Topic</typeparam>
        /// <returns></returns>
        string FindRoutingKey<T>();

        /// <summary>
        /// Find the queue name based on the message type and subscription name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionName"></param>
        /// <returns></returns>
        string FindQueueName<T>(string subscriptionName);
    }
}
