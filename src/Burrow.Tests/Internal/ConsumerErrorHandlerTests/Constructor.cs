using System;
using Burrow.Internal;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsumerErrorHandlerTests
{
    [TestFixture]
    public class Constructor
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_conectionFactory_is_null()
        {
            new ConsumerErrorHandler(null, NSubstitute.Substitute.For<ISerializer>(), NSubstitute.Substitute.For<IRabbitWatcher>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_serializer_is_null()
        {
            new ConsumerErrorHandler(NSubstitute.Substitute.For<IDurableConnection>(), null, NSubstitute.Substitute.For<IRabbitWatcher>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_watcher_is_null()
        {
            new ConsumerErrorHandler(NSubstitute.Substitute.For<IDurableConnection>(), NSubstitute.Substitute.For<ISerializer>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming