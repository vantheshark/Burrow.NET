using System;
using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcServerCoordinatorTests
{
    [TestFixture]
    public class Constructor
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_real_instance()
        {
            // Arrange
            new BurrowRpcServerCoordinator<ISomeService>(null, "request-queue-name", "queue-connnection-string");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_route_finder()
        {
            // Arrange
            IRpcRouteFinder routeFinder = null;
            new BurrowRpcServerCoordinator<ISomeService>(NSubstitute.Substitute.For<ISomeService>(), routeFinder, "queue-connnection-string");
        }

        [Test]
        public void Should_allow_using_null_connectionString()
        {
            // Arrange
            string connectionString = null;
            InternalDependencies.RpcQueueHelper = NSubstitute.Substitute.For<IRpcQueueHelper>();
            new BurrowRpcServerCoordinator<ISomeService>(NSubstitute.Substitute.For<ISomeService>(), connectionString);
        }
    }
}
// ReSharper restore InconsistentNaming