using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultFanoutRpcRequestRouteFinderTests
{
    [TestFixture]
    public class PropertyRequestQueue
    {
        [Test]
        public void Should_return_provided_request_queue_if_it_not_null_or_empty()
        {
            // Arrange
            var routeFinder = new DefaultFanoutRpcRequestRouteFinder<ISomeService>(requestQueueName: "RequestQueue");

            // Action
            var requestQueue = routeFinder.RequestQueue;

            // Assert
            Assert.AreEqual("RequestQueue", requestQueue);
        }

        [Test]
        public void Should_return_base_request_queue_name_if_provided_request_queue_is_null_or_empty()
        {
            // Arrange
            var routeFinder = new DefaultFanoutRpcRequestRouteFinder<ISomeService>(requestQueueName: "");

            // Action
            var requestQueue = routeFinder.RequestQueue;

            // Assert
            Assert.AreEqual("Burrow.Queue.Rpc.ISomeService.Requests", requestQueue);
        }

        [Test]
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