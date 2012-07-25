using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using NSubstitute;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestClass]
    public class MethodAck
    {
        [TestMethod]
        public void Should_call_ack_on_nested_subscriptions()
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
            subs.Ack("ConsumerTag", 1);

            // Assert
            channel.Received().BasicAck(1, false);
        }


        [TestMethod]
        public void Should_call_ack_on_nested_subscriptions_with_all_delivery_tag()
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
            subs.Ack("ConsumerTag", new[] { (ulong)1, (ulong)2, (ulong)3, (ulong)4, (ulong)5 });

            // Assert
            channel.Received().BasicAck(1, false);
            channel.Received().BasicAck(2, false);
            channel.Received().BasicAck(3, false);
            channel.Received().BasicAck(4, false);
            channel.Received().BasicAck(5, false);
        }
    }
}
// ReSharper restore InconsistentNaming