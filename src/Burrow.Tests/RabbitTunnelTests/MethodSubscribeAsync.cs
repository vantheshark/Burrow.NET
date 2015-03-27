using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestFixture]
    public class MethodSubscribeAsync
    {
        [Test]
        public void Should_create_subscriptions_to_queue()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);

            // Action
            tunnel.SubscribeAsync<Customer>("subscriptionName", x => { }, 1);

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
            tunnel.SubscribeAsync<Customer>("subscriptionName", (x, y) => { }, 1);

            // Assert
            newChannel.Received().BasicQos(0, (ushort)Global.PreFetchSize, false);
            newChannel.Received().BasicConsume("Queue", false, Arg.Is<string>(x => x.StartsWith("subscriptionName-")), Arg.Any<IBasicConsumer>());
        }
    }
}
// ReSharper restore InconsistentNaming