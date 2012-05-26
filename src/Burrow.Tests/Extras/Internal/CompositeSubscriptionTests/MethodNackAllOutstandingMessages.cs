using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestClass]
    public class MethodNackAllOutstandingMessages
    {
        [TestMethod]
        public void Should_call_NackAllOutstandingMessages_on_nested_subscription()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
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
            subs.NackAllOutstandingMessages("ConsumerTag", true);

            // Assert
            channel.Received().BasicNack(0, true, true);
        }
    }
}
// ReSharper restore InconsistentNaming