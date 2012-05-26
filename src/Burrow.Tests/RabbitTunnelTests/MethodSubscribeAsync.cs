using System;
using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodSubscribeAsync
    {
        [TestMethod]
        public void Should_create_subscriptions_to_queue()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.SubscribeAsync<Customer>("subscriptionName", x => { });

            // Assert
            newChannel.Received().BasicQos(0, Global.DefaultConsumerBatchSize, false);
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
            tunnel.SubscribeAsync<Customer>("subscriptionName", (x, y) => { });

            // Assert
            newChannel.Received().BasicQos(0, Global.DefaultConsumerBatchSize, false);
            newChannel.Received().BasicConsume("Queue", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }
    }
}
// ReSharper restore InconsistentNaming