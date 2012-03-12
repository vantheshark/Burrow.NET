
namespace Burrow.Internal
{
    internal class DefaultRouteFinder : IRouteFinder
    {
        public string FindExchangeName<T>()
        {
            return "Burrow.Exchange";
        }

        public string FindRoutingKey<T>()
        {
            return typeof (T).Name;
        }

        public string FindQueueName<T>(string subscriptionName)
        {
            return string.IsNullOrEmpty(subscriptionName)
                ? string.Format("Burrow.Queue.{0}", typeof (T).Name)
                : string.Format("Burrow.Queue.{0}.{1}", subscriptionName, typeof (T).Name);
        }
    }
}
