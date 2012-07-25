using System;
using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.RabbitTunnelWithPriorityQueuesSupportTests
{
    [TestClass]
    public class MethodSubscribe
    {
        [TestMethod]
        public void Should_create_subscriptions_to_priority_queues()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.Subscribe<Customer>("subscriptionName", 3, x => { });

            // Assert
            newChannel.Received(4).BasicQos(0, Global.PreFetchSize, false);
            newChannel.Received().BasicConsume("Queue_Priority0", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority1", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority2", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority3", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }


        [TestMethod]
        public void Should_return_composite_subscription()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelWithPriorityQueuesSupportForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            var subs = tunnel.Subscribe<Customer>("subscriptionName", 3, (x, y) => { });

            // Assert
            newChannel.Received(4).BasicQos(0, Global.PreFetchSize, false);
            newChannel.Received().BasicConsume("Queue_Priority0", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority1", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority2", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            newChannel.Received().BasicConsume("Queue_Priority3", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
            Assert.IsInstanceOfType(subs, typeof(CompositeSubscription));
            Assert.AreEqual(4, subs.Count);
        }

        [TestMethod]
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
            newChannel.ModelShutdown += Raise.Event<ModelShutdownEventHandler>(newChannel, new ShutdownEventArgs(ShutdownInitiator.Peer, 0, "Shutdown"));

            // Assert
            Assert.IsTrue(call);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
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