using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using NSubstitute;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestClass]
    public class MethodNack
    {
        [TestMethod]
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


        [TestMethod]
        public void Should_call_Nack_on_nested_subscriptions_with_the_max_value_of_delivery_tag()
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