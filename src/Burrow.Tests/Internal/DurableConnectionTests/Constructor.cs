using System;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_retryPolicy_is_null()
        {
            new DurableConnection(null, NSubstitute.Substitute.For<IRabbitWatcher>(), NSubstitute.Substitute.For<ConnectionFactory>());
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_watcher_is_null()
        {
            new DurableConnection(NSubstitute.Substitute.For<IRetryPolicy>(), null, NSubstitute.Substitute.For<ConnectionFactory>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_ConnectionFactory_is_null()
        {
            new DurableConnection(NSubstitute.Substitute.For<IRetryPolicy>(), NSubstitute.Substitute.For<IRabbitWatcher>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming