using System;
using System.Configuration;
using Burrow.Extras;
using Burrow.Publisher.Models;

namespace Burrow.Publisher
{
    public static class RabbitSetupTest
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

        private static readonly ExchangeSetupData ExchangeSetupData = new ExchangeSetupData();
        

        private static readonly QueueSetupData QueueSetupData = new QueueSetupData
        {
            SubscriptionName = "BurrowTestApp",
            MessageTimeToLive = 100000
        };

        public static void CreateExchangesAndQueues()
        {
            Func<string, string, IRouteFinder> factory = (x, y) => new TestRouteFinder();
            var setup = new RabbitSetup(factory, Global.DefaultWatcher, ConfigurationManager.ConnectionStrings["RabbitMQ"].ToString(), "TEST");
            setup.SetupExchangeAndQueueFor<Bunny>(ExchangeSetupData, QueueSetupData);

        }

        public static void DestroyExchangesAndQueues()
        {
            Func<string, string, IRouteFinder> factory = (x, y) => new TestRouteFinder();
            var setup = new RabbitSetup(factory, Global.DefaultWatcher, ConfigurationManager.ConnectionStrings["RabbitMQ"].ToString(), "TEST");
            setup.Destroy<Bunny>(ExchangeSetupData, QueueSetupData);
        }

        public static void CreateExchangesAndQueues(string exchangeName, string queueName, string routingKey)
        {
            Func<string, string, IRouteFinder> factory = (x, y) => new ConstantRouteFinder(exchangeName, queueName, routingKey);
            var setup = new RabbitSetup(factory, Global.DefaultWatcher, ConfigurationManager.ConnectionStrings["RabbitMQ"].ToString(), "TEST");
            setup.SetupExchangeAndQueueFor<Bunny>(ExchangeSetupData, QueueSetupData);
        }

        public static void DestroyExchangesAndQueues(string exchangeName, string queueName)
        {
            Func<string, string, IRouteFinder> factory = (x, y) => new ConstantRouteFinder(exchangeName, queueName, null);
            var setup = new RabbitSetup(factory, Global.DefaultWatcher, ConfigurationManager.ConnectionStrings["RabbitMQ"].ToString(), "TEST");
            setup.Destroy<Bunny>(ExchangeSetupData, QueueSetupData);
        }
    }
}
