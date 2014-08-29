using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_channel()
        {
            // Action
            new BurrowConsumer(null, NSubstitute.Substitute.For<IMessageHandler>(),
                               NSubstitute.Substitute.For<IRabbitWatcher>(), false, 3);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_messageHandler()
        {
            // Action
            new BurrowConsumer(NSubstitute.Substitute.For<IModel>(), null,
                               NSubstitute.Substitute.For<IRabbitWatcher>(), false, 3);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_watcher()
        {
            // Action
            new BurrowConsumer(NSubstitute.Substitute.For<IModel>(), NSubstitute.Substitute.For<IMessageHandler>(),
                               null, false, 3);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Should_throw_exception_if_provide_batchSize_less_than_1()
        {
            // Action
            new BurrowConsumer(NSubstitute.Substitute.For<IModel>(), NSubstitute.Substitute.For<IMessageHandler>(),
                               NSubstitute.Substitute.For<IRabbitWatcher>(), false, 0);
        }
    }
}
// ReSharper restore InconsistentNaming