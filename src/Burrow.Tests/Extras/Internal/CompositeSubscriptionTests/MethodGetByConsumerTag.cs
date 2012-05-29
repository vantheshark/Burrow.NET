using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestClass]
    public class MethodGetByConsumerTag
    {
        [TestMethod]
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