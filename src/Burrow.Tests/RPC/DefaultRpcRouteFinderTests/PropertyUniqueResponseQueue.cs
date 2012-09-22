using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultRpcRouteFinderTests
{
    [TestClass]
    public class PropertyUniqueResponseQueue
    {
        [TestMethod]
        public void Should_return_name_with_type()
        {
            // Arrange
            var routeFinder = new DefaultRpcRouteFinder<ISomeService>();

            // Action
            var responseQueue = routeFinder.UniqueResponseQueue;

            // Assert
            Assert.AreEqual("Burrow.Queue.Rpc.ISomeService.Responses", responseQueue);
        }

        [TestMethod]
        public void Should_return_name_with_type_and_client_name()
        {
            // Arrange
            var routeFinder = new DefaultRpcRouteFinder<ISomeService>(clientName: "UnitTest");

            // Action
            var responseQueue = routeFinder.UniqueResponseQueue;

            // Assert
            Assert.AreEqual("Burrow.Queue.Rpc.UnitTest.ISomeService.Responses", responseQueue);
        }
    }
}
// ReSharper restore InconsistentNaming