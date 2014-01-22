using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestClass]
    public class MethodCancel
    {
        [TestMethod]
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