using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using System;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestClass]
    public class MethodNack
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_delivery_tags_is_null()
        {
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var subscription = new Subscription(channel);

            // Action
            subscription.Nack(null, false);
        }
    }
}
// ReSharper restore InconsistentNaming