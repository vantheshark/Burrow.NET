using System;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcServerCoordinatorTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_real_instance()
        {
            // Arrange
            new BurrowRpcServerCoordinator<ISomeService>(null, "request-queue-name", "queue-connnection-string");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_route_finder()
        {
            // Arrange
            IRpcRouteFinder routeFinder = null;
            new BurrowRpcServerCoordinator<ISomeService>(NSubstitute.Substitute.For<ISomeService>(), routeFinder, "queue-connnection-string");
        }

        [TestMethod]
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