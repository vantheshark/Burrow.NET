using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using NSubstitute;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestClass]
    public class MethodCancelAll
    {
        [TestMethod]
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