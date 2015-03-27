using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestFixture]
    public class MethodCancelAll
    {
        [Test]
        public void Should_call_cancel_on_nested_subscriptions()
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
            subs.CancelAll();

            // Assert
            channel.Received().BasicCancel(subscription.ConsumerTag);
        }
    }
}
// ReSharper restore InconsistentNaming