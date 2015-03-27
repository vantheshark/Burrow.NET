using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestFixture]
    public class MethodCloseTunnel
    {
        [Test]
        public void Should_resubscribe_to_queues_after_reconnect_successfully()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Subscribe<Customer>("subscriptionName", x => { });
            tunnel.Subscribe<Customer>("subscriptionName2", x => { });

            // Action
            durableConnection.Disconnected += Raise.Event<Action>();
            durableConnection.Connected += Raise.Event<Action>();

            // Assert
            newChannel.Received(4).BasicConsume("Queue", false, Arg.Is<string>(x => x.StartsWith("subscriptionName")), Arg.Any<IBasicConsumer>());
        }

        [Test]
        public void Should_do_nothing_if_disposed()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Subscribe<Customer>("subscriptionName", x => { });
            tunnel.Dispose();

            // Action
            durableConnection.Disconnected += Raise.Event<Action>();

            // Assert
            newChannel.Received(2).Abort(); //1 for publishing channel, 1 for the above subscription
            newChannel.Received(2).Dispose(); //1 for publishing channel, 1 for the above subscription
        }
    }
}
// ReSharper restore InconsistentNaming