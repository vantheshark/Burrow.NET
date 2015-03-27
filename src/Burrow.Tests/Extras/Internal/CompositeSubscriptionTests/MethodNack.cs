using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestFixture]
    public class MethodNack
    {
        [Test]
        public void Should_call_Nack_on_nested_subscriptions()
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
            subs.Nack("ConsumerTag", 1, false);

            // Assert
            channel.Received().BasicNack(1, false, false);
        }


        [Test]
        public void Should_call_Nack_on_nested_subscriptions_with_all_delivery_tag()
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
            subs.Nack("ConsumerTag", new[] { (ulong)1, (ulong)2, (ulong)3, (ulong)4, (ulong)5 }, false);

            // Assert
            channel.Received().BasicNack(5, true, false);
        }
    }
}
// ReSharper restore InconsistentNaming