using Burrow.Tests.Extras.RabbitSetupTests;

using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestFixture]
    public class MethodSubscribe
    {
        [Test]
        public void Should_create_subscriptions_to_queues()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe<Customer>("subscriptionName", x => { });

            // Assert
            newChannel.Received().BasicQos(0, (ushort)Global.PreFetchSize, false);
            newChannel.Received().BasicConsume("Queue", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }


        [Test]
        public void Should_return_subscription()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            var subs = tunnel.Subscribe<Customer>("subscriptionName", (x, y) => { });

            // Assert
            newChannel.Received().BasicQos(0, (ushort)Global.PreFetchSize, false);
            newChannel.Received().BasicConsume("Queue", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            Assert.IsInstanceOfType(typeof(Subscription), subs);
        }

        [Test]
        public void Should_be_able_to_subscribe_with_provided_options()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRouteFinder>();
            routeFinder.FindQueueName<Customer>("subscriptionName").Returns("QueueByCustomRouteFinder");
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe(new SubscriptionOption<Customer>
            {
                SubscriptionName = "subscriptionName",
                MessageHandler = x => { },
                QueuePrefetchSize = 100,
                BatchSize = 100,
                RouteFinder = routeFinder
            });

            // Assert
            routeFinder.Received(1).FindQueueName<Customer>("subscriptionName");
            newChannel.Received().BasicQos(0, 100, false);
            newChannel.Received().BasicConsume("QueueByCustomRouteFinder", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }

        [Test]
        public void Should_warn_if_prefetch_size_is_too_large()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRouteFinder>();
            routeFinder.FindQueueName<Customer>("subscriptionName").Returns("QueueByCustomRouteFinder");
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe(new SubscriptionOption<Customer>
            {
                SubscriptionName = "subscriptionName",
                MessageHandler = x => { },
                QueuePrefetchSize = uint.MaxValue,
                RouteFinder = routeFinder
            });

            // Assert
            routeFinder.Received(1).FindQueueName<Customer>("subscriptionName");
            newChannel.Received().BasicQos(0, ushort.MaxValue, false);
            newChannel.Received().BasicConsume("QueueByCustomRouteFinder", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }
    }
}
// ReSharper restore InconsistentNaming