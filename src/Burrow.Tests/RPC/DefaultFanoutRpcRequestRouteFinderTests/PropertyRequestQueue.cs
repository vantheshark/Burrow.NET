using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultFanoutRpcRequestRouteFinderTests
{
    [TestClass]
    public class PropertyRequestQueue
    {
        [TestMethod]
        public void Should_return_provided_request_queue_if_it_not_null_or_empty()
        {
            // Arrange
            var routeFinder = new DefaultFanoutRpcRequestRouteFinder<ISomeService>(requestQueueName: "RequestQueue");

            // Action
            var requestQueue = routeFinder.RequestQueue;

            // Assert
            Assert.AreEqual("RequestQueue", requestQueue);
        }

        [TestMethod]
        public void Should_return_base_request_queue_name_if_provided_request_queue_is_null_or_empty()
        {
            // Arrange
            var routeFinder = new DefaultFanoutRpcRequestRouteFinder<ISomeService>(requestQueueName: "");

            // Action
            var requestQueue = routeFinder.RequestQueue;

            // Assert
            Assert.AreEqual("Burrow.Queue.Rpc.ISomeService.Requests", requestQueue);
        }

        [TestMethod]
        public void Should_return_request_queue_name_with_serverId_if_provided_request_queue_is_null_or_empty()
        {
            // Arrange
            var routeFinder = new DefaultFanoutRpcRequestRouteFinder<ISomeService>("UnitTest", requestQueueName: "");

            // Action
            var requestQueue = routeFinder.RequestQueue;

            // Assert
            Assert.AreEqual("Burrow.Queue.Rpc.UnitTest.ISomeService.Requests", requestQueue);
        }
    }
}
// ReSharper restore InconsistentNaming