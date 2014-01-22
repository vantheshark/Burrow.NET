using System.Configuration;
using Burrow.Extras;
using Burrow.Publisher.Models;

namespace Burrow.Publisher
{
    public static class RabbitSetupTest
    {
        private const string SubscriptionName = "BurrowTestApp";
        private static string _connectionString = ConfigurationManager.ConnectionStrings["RabbitMQ"].ToString();
        private static RabbitSetup setup = new RabbitSetup(_connectionString);

        private static readonly RouteSetupData RouteSetupData = new RouteSetupData
        {
            RouteFinder = new TestRouteFinder(),
            ExchangeSetupData = new ExchangeSetupData(),
            QueueSetupData = new QueueSetupData
            {
                MessageTimeToLive = 100000
            },
            SubscriptionName = SubscriptionName
        };

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


        public static void CreateExchangesAndQueues()
        {
            setup.CreateRoute<Bunny>(RouteSetupData);
        }

        public static void DestroyExchangesAndQueues()
        {
            setup.DestroyRoute<Bunny>(RouteSetupData);
        }

        public static void CreateExchangesAndQueues(string exchangeName, string queueName, string routingKey)
        {
            var customRouteData = new RouteSetupData
            {
                RouteFinder = new ConstantRouteFinder(exchangeName, queueName, routingKey),
                ExchangeSetupData = new ExchangeSetupData(),
                QueueSetupData = new QueueSetupData
                {
                    MessageTimeToLive = 100000
                },
                SubscriptionName = SubscriptionName
            };

            setup.CreateRoute<Bunny>(customRouteData);
        }

        public static void DestroyExchangesAndQueues(string exchangeName, string queueName)
        {
            var customRouteData = new RouteSetupData
            {
                RouteFinder = new ConstantRouteFinder(exchangeName, queueName, null),
                ExchangeSetupData = new ExchangeSetupData(),
                QueueSetupData = new QueueSetupData
                {
                    MessageTimeToLive = 100000
                },
                SubscriptionName = SubscriptionName
            };
            
            setup.DestroyRoute<Bunny>(customRouteData);
        }
    }
}
