using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestFixture]
    public class MethodNackAllOutstandingMessages
    {
        [Test]
        public void Should_call_NackAllOutstandingMessages_on_nested_subscription()
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
            subs.NackAllOutstandingMessages("ConsumerTag", true);

            // Assert
            channel.Received().BasicNack(0, true, true);
        }
    }
}
// ReSharper restore InconsistentNaming