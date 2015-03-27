
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestFixture]
    public class MethodNackAllUpTo
    {
        [Test]
        public void Should_Nack_with_provided_delivery_tag_with_multiple_flag()
        {
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var subscription = new Subscription(channel)
            {
                ConsumerTag = "ConsumerTag"
            };

            // Action
            subscription.NackAllUpTo(10, true);

            // Assert
            channel.Received(1).BasicNack(10, true, true);
        }
    }
}
// ReSharper restore InconsistentNaming