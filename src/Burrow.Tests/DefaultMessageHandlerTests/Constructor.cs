using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{
    [TestFixture]
    public class Constructor
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_action()
        {
            new DefaultMessageHandler<Customer>("SubscriptionName", null, Substitute.For<IConsumerErrorHandler>(), Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_consumerErrorHandler()
        {
            new DefaultMessageHandler<Customer>("SubscriptionName", Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), null, Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());
        }


        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_serializer()
        {
            new DefaultMessageHandler<Customer>("SubscriptionName", Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), Substitute.For<IConsumerErrorHandler>(), null, Substitute.For<IRabbitWatcher>());
        }


        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_rabbit_watcher()
        {
            new DefaultMessageHandler<Customer>("SubscriptionName", Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), Substitute.For<IConsumerErrorHandler>(), Substitute.For<ISerializer>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming