using System;
using System.Configuration;
using Burrow.Extras;
using Burrow.Publisher.Models;

namespace Burrow.Publisher
{
    public static class PriorityRabbitSetupTest
    {
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

        private static readonly ExchangeSetupData ExchangeSetupData = new HeaderExchangeSetupData();
        private static readonly QueueSetupData QueueSetupData = new PriorityQueueSetupData(3)
        {
            SubscriptionName = "BurrowTestApp",
            MessageTimeToLive = 1000 * 3600
        };

        public static void CreateExchangesAndQueues()
        {
            Func<string, string, IRouteFinder> factory = (environment, exchangeType) => new TestRouteFinder();
            var setup = new PriorityQueuesRabbitSetup(factory, Global.DefaultWatcher, ConfigurationManager.ConnectionStrings["RabbitMQ"].ToString(), "TEST");
            setup.SetupExchangeAndQueueFor<Bunny>(ExchangeSetupData, QueueSetupData);

        }

        public static void DestroyExchangesAndQueues()
        {
            Func<string, string, IRouteFinder> factory = (environment, exchangeType) => new TestRouteFinder();
            var setup = new PriorityQueuesRabbitSetup(factory, Global.DefaultWatcher, ConfigurationManager.ConnectionStrings["RabbitMQ"].ToString(), "TEST");
            setup.Destroy<Bunny>(ExchangeSetupData, QueueSetupData);
        }
    }
}
