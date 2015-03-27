using System;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerFactoryTests
{
    [TestFixture]
    public class Constructor
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_use_null_consumerErrorHandler()
        {
            // Action
            new DefaultMessageHandlerFactory(null, NSubstitute.Substitute.For<ISerializer>(), NSubstitute.Substitute.For<IRabbitWatcher>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_use_null_serializer()
        {
            // Action
            new DefaultMessageHandlerFactory(NSubstitute.Substitute.For<IConsumerErrorHandler>(), null, NSubstitute.Substitute.For<IRabbitWatcher>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_use_null_watcher()
        {
            // Action
            new DefaultMessageHandlerFactory(NSubstitute.Substitute.For<IConsumerErrorHandler>(), NSubstitute.Substitute.For<ISerializer>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming