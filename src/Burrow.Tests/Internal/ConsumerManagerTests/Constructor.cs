using System;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsumerManagerTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_watcher_is_null()
        {
            new ConsumerManager(null, NSubstitute.Substitute.For<IMessageHandlerFactory>(), NSubstitute.Substitute.For<ISerializer>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_handler_is_null()
        {
            new ConsumerManager(NSubstitute.Substitute.For<IRabbitWatcher>(), null, NSubstitute.Substitute.For<ISerializer>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_serialier_is_null()
        {
            new ConsumerManager(NSubstitute.Substitute.For<IRabbitWatcher>(), NSubstitute.Substitute.For<IMessageHandlerFactory>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming