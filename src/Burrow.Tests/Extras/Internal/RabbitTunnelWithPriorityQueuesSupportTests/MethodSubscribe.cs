using System;
using System.Globalization;
using Burrow.Extras;
using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.RabbitTunnelWithPriorityQueuesSupportTests
{
    [TestFixture]
    public class MethodSubscribe
    {
        [Test]
        public void Should_create_subscriptions_to_priority_queues()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe<Customer>("subscriptionName", 3, x => { });

            // Assert
            newChannel.Received(4).BasicQos(0, (ushort)Global.PreFetchSize, false);
            newChannel.Received().BasicConsume("Queue_Priority0", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority1", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority2", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority3", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }

        [Test]
        public void Should_be_able_to_use_custom_route_finder_and_prefix_convention()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRouteFinder>();
            var convention = Substitute.For<IPriorityQueueSuffix>();
            convention.Get(Arg.Any<Type>(), Arg.Any<uint>()).Returns(callInfo => "_P" + callInfo.Arg<uint>().ToString(CultureInfo.InvariantCulture));
            routeFinder.FindQueueName<Customer>(Arg.Any<string>()).Returns("Q");


            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe(new PrioritySubscriptionOption<Customer>
                                           {
                                               SubscriptionName = "subscriptionName",
                                               RouteFinder = routeFinder,
                                               MessageHandler = x => { },
                                               BatchSize = 1,
                                               MaxPriorityLevel = 3,
                                               QueueSuffixNameConvention = convention
                                           });

            // Assert
            newChannel.Received().BasicConsume("Q_P0", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Q_P1", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Q_P2", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Q_P3", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }

        [Test]
        public void Should_be_able_to_use_custom_prefetchSize()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRouteFinder>();
            var convention = Substitute.For<IPriorityQueueSuffix>();
            convention.Get(Arg.Any<Type>(), Arg.Any<uint>()).Returns(callInfo => "_P" + callInfo.Arg<uint>().ToString(CultureInfo.InvariantCulture));
            routeFinder.FindQueueName<Customer>(Arg.Any<string>()).Returns("Q");


            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe(new PrioritySubscriptionOption<Customer>
            {
                SubscriptionName = "subscriptionName",
                MessageHandler = x => { },
                BatchSize = 1,
                MaxPriorityLevel = 3,
                QueuePrefetchSize = 2,
                QueuePrefetchSizeSelector = x => x + 10
            });

            // Assert
            newChannel.Received(1).BasicQos(0, 10, false);
            newChannel.Received(1).BasicQos(0, 11, false);
            newChannel.Received(1).BasicQos(0, 12, false);
            newChannel.Received(1).BasicQos(0, 13, false);
        }

        [Test]
        public void Should_use_default_prefetch_size_if_lt_0()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRouteFinder>();
            var convention = Substitute.For<IPriorityQueueSuffix>();
            convention.Get(Arg.Any<Type>(), Arg.Any<uint>()).Returns(callInfo => "_P" + callInfo.Arg<uint>().ToString(CultureInfo.InvariantCulture));
            routeFinder.FindQueueName<Customer>(Arg.Any<string>()).Returns("Q");


            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe(new PrioritySubscriptionOption<Customer>
            {
                SubscriptionName = "subscriptionName",
                MessageHandler = x => { },
                BatchSize = 1,
                MaxPriorityLevel = 3,
                QueuePrefetchSize = 2,
                QueuePrefetchSizeSelector = x => 0
            });
            // Assert
            newChannel.Received(4).BasicQos(0, (ushort)Global.PreFetchSize, false);
        }

        [Test]
        public void Should_use_max_ushort_prefetch_size_if_is_too_big()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRouteFinder>();
            var convention = Substitute.For<IPriorityQueueSuffix>();
            convention.Get(Arg.Any<Type>(), Arg.Any<uint>()).Returns(callInfo => "_P" + callInfo.Arg<uint>().ToString(CultureInfo.InvariantCulture));
            routeFinder.FindQueueName<Customer>(Arg.Any<string>()).Returns("Q");


            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe(new PrioritySubscriptionOption<Customer>
            {
                SubscriptionName = "subscriptionName",
                MessageHandler = x => { },
                BatchSize = 1,
                MaxPriorityLevel = 3,
                QueuePrefetchSize = 2,
                QueuePrefetchSizeSelector = x => uint.MaxValue
            });
            // Assert
            newChannel.Received(4).BasicQos(0, ushort.MaxValue, false);
        }


        [Test]
        public void Should_return_composite_subscription()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            var subs = tunnel.Subscribe<Customer>("subscriptionName", 3, (x, y) => { });

            // Assert
            newChannel.Received(4).BasicQos(0, (ushort)Global.PreFetchSize, false);
            newChannel.Received().BasicConsume("Queue_Priority0", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority1", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority2", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority3", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            Assert.IsInstanceOfType(typeof(CompositeSubscription), subs);
            Assert.AreEqual(4, subs.Count);
        }

        [Test]
        public void Should_register_ModelShutdown_event_on_each_created_channel()
        {
            // Arrange
            var call = false;
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.ConsumerDisconnected += s => { call = true; };
            tunnel.Subscribe<Customer>("subscriptionName", 3, (x, y) => { });
            
            // Action
            newChannel.ModelShutdown += Raise.EventWith(newChannel, new ShutdownEventArgs(ShutdownInitiator.Peer, 0, "Shutdown"));

            // Assert
            Assert.IsTrue(call);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Should_throw_exception_if_provide_invalid_Comparer()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.ConsumerDisconnected += s => {};

            // Action
            //tunnel.Subscribe<Customer>("subscriptionName", 3, (x, y) => { }, typeof(PriorityComparer<>));
            tunnel.Subscribe<Customer>("subscriptionName", 3, (x, y) => { }, typeof(Customer));
        }
    }
}
// ReSharper restore InconsistentNaming