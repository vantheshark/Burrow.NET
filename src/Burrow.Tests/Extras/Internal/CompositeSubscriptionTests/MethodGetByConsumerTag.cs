using Burrow.Extras.Internal;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestFixture]
    public class MethodGetByConsumerTag
    {
        [Test]
        public void Should_return_subscription_by_consumer_tag()
        {
            // Arrange
            var subs = new CompositeSubscription();
            var subscription = new Subscription
            {
                ConsumerTag = "ConsumerTag",
                QueueName = "QueueName",
                SubscriptionName = "SubscriptionName"
            };
            subs.AddSubscription(subscription);

            // Action & Assert
            Assert.AreSame(subscription, subs.GetByConsumerTag("ConsumerTag"));
            Assert.IsNull(subs.GetByConsumerTag("ConsumerTag2"));
        }
    }
}
// ReSharper restore InconsistentNaming