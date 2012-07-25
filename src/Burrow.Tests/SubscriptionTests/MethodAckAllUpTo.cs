using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestClass]
    public class MethodAckAllUpTo
    {
        [TestMethod]
        public void Should_ack_with_provided_delivery_tag_with_multiple_flag()
        {
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var subscription = new Subscription(channel);

            // Action
            subscription.AckAllUpTo(10);

            // Assert
            channel.Received(1).BasicAck(10, true);
        }
    }
}
// ReSharper restore InconsistentNaming