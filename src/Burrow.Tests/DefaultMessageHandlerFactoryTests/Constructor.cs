using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerFactoryTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_use_null_consumerErrorHandler()
        {
            // Action
            new DefaultMessageHandlerFactory(null, NSubstitute.Substitute.For<IRabbitWatcher>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_use_null_watcher()
        {
            // Action
            new DefaultMessageHandlerFactory(NSubstitute.Substitute.For<IConsumerErrorHandler>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming