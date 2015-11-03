
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
                ? $"Burrow.Queue.{typeof (T).Name}"
                : $"Burrow.Queue.{subscriptionName}.{typeof (T).Name}";
        }
    }
}
