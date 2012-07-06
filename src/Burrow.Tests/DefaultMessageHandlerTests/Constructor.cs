using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_action()
        {
            new DefaultMessageHandler<Customer>("SubscriptionName", null, Substitute.For<IConsumerErrorHandler>(), Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_consumerErrorHandler()
        {
            new DefaultMessageHandler<Customer>("SubscriptionName", Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), null, Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_serializer()
        {
            new DefaultMessageHandler<Customer>("SubscriptionName", Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), Substitute.For<IConsumerErrorHandler>(), null, Substitute.For<IRabbitWatcher>());
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_rabbit_watcher()
        {
            new DefaultMessageHandler<Customer>("SubscriptionName", Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), Substitute.For<IConsumerErrorHandler>(), Substitute.For<ISerializer>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming