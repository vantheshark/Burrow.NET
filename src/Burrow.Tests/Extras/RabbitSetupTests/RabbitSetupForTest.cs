using System;
using Burrow.Extras;
using NSubstitute;
using RabbitMQ.Client;

namespace Burrow.Tests.Extras.RabbitSetupTests
{
    public class RabbitSetupForTest : RabbitSetup
    {
        public IRabbitWatcher Watcher { get; set; }

        public RabbitSetupForTest(Func<string, string, IRouteFinder> routeFinderFactory, IRabbitWatcher watcher, string connectionString, string environment)
            : base(routeFinderFactory, watcher, connectionString, environment)
        {
            Watcher = watcher;
        }

        public ConnectionFactory ConnectionFactory
        {
            set { _connectionFactory = value; }
        }

        public static RabbitSetupForTest CreateRabbitSetup(IModel model)
        {
            var connection = Substitute.For<IConnection>();
            connection.CreateModel().Returns(model);
            var routeFinder = Substitute.For<IRouteFinder>();
            routeFinder.FindExchangeName<Customer>().Returns("Exchange.Customer");
            routeFinder.FindQueueName<Customer>(null).ReturnsForAnyArgs("Queue.Customer");
            routeFinder.FindRoutingKey<Customer>().Returns("Customer");

            var connectionFactory = Substitute.For<ConnectionFactory>();
            connectionFactory.CreateConnection().Returns(connection);
            Func<string, string, IRouteFinder> factory = (x, y) => routeFinder;
            var watcher = Substitute.For<IRabbitWatcher>();
            var setup = new RabbitSetupForTest(factory, watcher, "", "UNITTEST") { ConnectionFactory = connectionFactory };
            return setup;
        }

        public new void BindQueue<T>(IModel model, QueueSetupData queue, string exchangeName, string queueName, string routingKey)
        {
            base.BindQueue<T>(model, queue, exchangeName, queueName, routingKey);
        }

        public new void DeclareExchange(ExchangeSetupData exchange, IModel model, string exchangeName)
        {
            base.DeclareExchange(exchange, model, exchangeName);
        }
    }
}
