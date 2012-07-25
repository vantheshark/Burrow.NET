using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestClass]
    public class MethodAckAllUpTo
    {
        [TestMethod]
        public void Should_ack_with_provided_delivery_tag_with_multiple_flag()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var subs = new CompositeSubscription();
            var subscription = new Subscription
            {
                ConsumerTag = "ConsumerTag",
                QueueName = "QueueName",
                SubscriptionName = "SubscriptionName"
            };
            subscription.SetChannel(channel);
            subs.AddSubscription(subscription);

            // Action
            subs.AckAllUpTo("ConsumerTag", 10);

            // Assert
            channel.Received().BasicAck(10, true);
        }
    }
}
// ReSharper restore InconsistentNaming