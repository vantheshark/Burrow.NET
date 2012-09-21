using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodSubscribe
    {
        [TestMethod]
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


        [TestMethod]
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
            Assert.IsInstanceOfType(subs, typeof(Subscription));
        }
    }
}
// ReSharper restore InconsistentNaming