using System;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcRouteFinderTests
{
    [TestClass]
    public class MethodFindExchangeName
    {
        [TestMethod]
        public void Should_return_empty_string_if_type_is_RpcRequest()
        {
            // Arrange
            var routeFinder = new BurrowRpcRouteFinder();

            // Action
            var exchangeName = routeFinder.FindExchangeName<RpcRequest>();

            // Assert
            Assert.AreEqual(string.Empty, exchangeName);
        }

        [TestMethod]
        public void Should_return_empty_string_if_type_is_RpcResponse()
        {
            // Arrange
            var routeFinder = new BurrowRpcRouteFinder();

            // Action
            var exchangeName = routeFinder.FindExchangeName<RpcResponse>();

            // Assert
            Assert.AreEqual(string.Empty, exchangeName);
        }

        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void Should_throw_exception_if_type_is_not_request_nor_response()
        {
            // Arrange
            var routeFinder = new BurrowRpcRouteFinder();

            // Action
            var exchangeName = routeFinder.FindExchangeName<SomeMessage>();

            // Assert
            Assert.AreEqual(string.Empty, exchangeName);
        }
    }
}
// ReSharper restore InconsistentNaming