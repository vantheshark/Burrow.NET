using System.Configuration;
using Burrow.Extras;
using Burrow.Publisher.Models;

namespace Burrow.Publisher
{
    public static class PriorityRabbitSetupTest
    {
        private static string _connectionString = ConfigurationManager.ConnectionStrings["RabbitMQ"].ToString();
        private class TestRouteFinder : IRouteFinder
        {
            public string FindExchangeName<T>()
            {
                return "Burrow.Exchange";
            }

            public string FindRoutingKey<T>()
            {
                return typeof(T).Name;
            }

            public string FindQueueName<T>(string subscriptionName)
            {
                return string.IsNullOrEmpty(subscriptionName)
                    ? string.Format("Burrow.Queue.{0}", typeof(T).Name)
                    : string.Format("Burrow.Queue.{0}.{1}", subscriptionName, typeof(T).Name);
            }
        }

        private const string SubscriptionName = "BurrowTestApp";
        
        private static readonly RouteSetupData RouteSetupData = new RouteSetupData
        {
            RouteFinder = new TestRouteFinder(),
            ExchangeSetupData = new HeaderExchangeSetupData(),
            QueueSetupData = new PriorityQueueSetupData(3)
            {
                MessageTimeToLive = 1000 * 3600,
                DeadLetterExchange = "",
                DeadLetterRoutingKey = "Burrow.Queue.Error" // Publishing dead letter message to empty exchange with the routing key like this will eventually make the msg go to that error queue
            },
            SubscriptionName = SubscriptionName
        };

        public static void CreateExchangesAndQueues()
        {
            var setup = new PriorityQueuesRabbitSetup(_connectionString);
            setup.CreateRoute<Bunny>(RouteSetupData);

        }

        public static void DestroyExchangesAndQueues()
        {
            var setup = new PriorityQueuesRabbitSetup(_connectionString);
            setup.DestroyRoute<Bunny>(RouteSetupData);
        }
    }
}
