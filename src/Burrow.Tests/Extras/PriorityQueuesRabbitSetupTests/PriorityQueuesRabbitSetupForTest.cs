using System;
using Burrow.Extras;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using RabbitMQ.Client;

namespace Burrow.Tests.Extras.PriorityQueuesRabbitSetupTests
{
    public class PriorityQueuesRabbitSetupForTest : PriorityQueuesRabbitSetup
    {
        public PriorityQueuesRabbitSetupForTest(Func<string, string, IRouteFinder> routeFinderFactory, IRabbitWatcher watcher, string connectionString, string environment)
            : base(routeFinderFactory, watcher, connectionString, environment)
        {
        }

        public ConnectionFactory ConnectionFactory
        {
            set { _connectionFactory = value; }
        }

        public static PriorityQueuesRabbitSetupForTest CreateRabbitSetup(IModel model)
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
            var setup = new PriorityQueuesRabbitSetupForTest(factory, watcher, "", "UNITTEST") { ConnectionFactory = connectionFactory };
            return setup;
        }
    }
}
