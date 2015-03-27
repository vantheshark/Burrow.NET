using System.Collections.Generic;
using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestFixture]
    public class MethodAck
    {
        [Test]
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


        [Test]
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
            Subscription.OutstandingDeliveryTags["ConsumerTag"] = new List<ulong>();
            Subscription.OutstandingDeliveryTags["ConsumerTag"].AddRange(new ulong[] { 1, 2, 3, 4, 5 });
            subs.Ack("ConsumerTag", new[] { (ulong)1, (ulong)2, (ulong)3, (ulong)4, (ulong)5 });
            

            // Assert
            channel.Received().BasicAck(5, true);
        }
    }
}
// ReSharper restore InconsistentNaming