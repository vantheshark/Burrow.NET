namespace Burrow
{
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
        /// Find the queue name based on the message type and subscribtion name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscribtionName"></param>
        /// <returns></returns>
        string FindQueueName<T>(string subscribtionName);
    }
}
