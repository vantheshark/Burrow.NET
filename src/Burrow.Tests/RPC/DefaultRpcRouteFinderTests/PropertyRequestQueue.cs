using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultRpcRouteFinderTests
{
    [TestFixture]
    public class PropertyRequestQueue
    {
        [Test]
        public void Should_return_name_with_type()
        {
            // Arrange
            var routeFinder = new DefaultRpcRouteFinder<ISomeService>(clientName: "UnitTest");

            // Action
            var requestQueue = routeFinder.RequestQueue;

            // Assert
            Assert.AreEqual("Burrow.Queue.Rpc.ISomeService.Requests", requestQueue);
        }


        [Test]
        public void Should_return_request_queue_if_provided()
        {
            // Arrange
            var routeFinder = new DefaultRpcRouteFinder<ISomeService>(requestQueueName: "RequestQueue");

            // Action
            var requestQueue = routeFinder.RequestQueue;

            // Assert
            Assert.AreEqual("RequestQueue", requestQueue);
        }
    }
}
// ReSharper restore InconsistentNaming