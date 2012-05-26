using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Burrow.Internal;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsumerErrorHandlerTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_conectionFactory_is_null()
        {
            new ConsumerErrorHandler(null, NSubstitute.Substitute.For<ISerializer>(), NSubstitute.Substitute.For<IRabbitWatcher>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_serializer_is_null()
        {
            new ConsumerErrorHandler(NSubstitute.Substitute.For<ConnectionFactory>(), null, NSubstitute.Substitute.For<IRabbitWatcher>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_watcher_is_null()
        {
            new ConsumerErrorHandler(NSubstitute.Substitute.For<ConnectionFactory>(), NSubstitute.Substitute.For<ISerializer>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming