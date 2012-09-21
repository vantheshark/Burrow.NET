using System;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcRouteFinderTests
{
    [TestClass]
    public class MethodFindQueueName
    {
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void Should_throw_exception_if_type_is_not_request_nor_response()
        {
            // Arrange
            var routeFinder = new BurrowRpcRouteFinder();

            // Action
            routeFinder.FindQueueName<SomeMessage>("");
        }


        [TestMethod]
        public void Should_return_valid_string_if_type_is_Request_or_Response()
        {
            // Arrange
            var routeFinder = new BurrowRpcRouteFinder();

            // Action
            var requestQueue = routeFinder.FindQueueName<RpcRequest>("UnitTest");
            var responseQueue = routeFinder.FindQueueName<RpcResponse>("UnitTest");

            // Assert
            Assert.AreEqual("Burrow.Queue.Rpc.UnitTest.Requests", requestQueue);
            Assert.AreEqual("Burrow.Queue.Rpc.UnitTest.Responses", responseQueue);

        }
    }
}
// ReSharper restore InconsistentNaming