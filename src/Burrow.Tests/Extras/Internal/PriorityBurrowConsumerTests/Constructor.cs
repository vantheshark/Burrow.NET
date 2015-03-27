using System;
using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestFixture]
    public class Constructor
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_channel()
        {
            // Action
            new PriorityBurrowConsumer(null, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), false, 1);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_MessageHandler()
        {
            // Action
            new PriorityBurrowConsumer(Substitute.For<IModel>(), null, Substitute.For<IRabbitWatcher>(), false, 1);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_watcher()
        {
            // Action
            new PriorityBurrowConsumer(Substitute.For<IModel>(), Substitute.For<IMessageHandler>(), null, false, 1);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Should_throw_exception_if_provide_less_than_or_equal_0_batch_size()
        {
            // Action
            new PriorityBurrowConsumer(Substitute.For<IModel>(), Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), false, 0);
        }
    }
}
// ReSharper restore InconsistentNaming