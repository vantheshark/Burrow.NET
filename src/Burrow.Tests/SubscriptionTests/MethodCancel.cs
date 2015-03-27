
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestFixture]
    public class MethodCancel
    {
        [Test]
        public void Should_cancel_the_subscription()
        {
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var subscription = new Subscription(channel)
                                   {
                                       ConsumerTag = "ConsumerTag"
                                   };

            // Action
            subscription.Cancel();

            // Assert
            channel.Received(1).BasicCancel(subscription.ConsumerTag);
        }
    }
}
// ReSharper restore InconsistentNaming