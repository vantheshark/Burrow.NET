using System;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcRouteFinderAdapterTests
{
    [TestClass]
    public class MethodFindExchangeName
    {
        private IRpcRouteFinder _routeFinder;

        [TestInitializeAttribute]
        public void Init()
        {
            _routeFinder = Substitute.For<IRpcRouteFinder>();
            _routeFinder.RequestExchangeName.Returns("RequestExchange");
            _routeFinder.RequestQueue.Returns("RequestQueue");
            _routeFinder.UniqueResponseQueue.Returns("ResposeQueue");
        }

        [TestMethod]
        public void Should_return_exchange_name_of_rpc_route_finder()
        {
            // Arrange
            var routeFinder = new RpcRouteFinderAdapter(_routeFinder);

            // Action
            var exchangeName = routeFinder.FindExchangeName<RpcRequest>();

            // Assert
            Assert.AreEqual("RequestExchange", exchangeName);
        }

        [TestMethod]
        public void Should_return_empty_string_if_type_is_RpcResponse()
        {
            // Arrange
            var routeFinder = new RpcRouteFinderAdapter(_routeFinder);

            // Action
            var exchangeName = routeFinder.FindExchangeName<RpcResponse>();

            // Assert
            Assert.AreEqual(string.Empty, exchangeName);
        }

        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void Should_throw_exception_if_type_is_not_request_nor_response()
        {
            // Arrange
            var routeFinder = new RpcRouteFinderAdapter(_routeFinder);

            // Action
            var exchangeName = routeFinder.FindExchangeName<SomeMessage>();

            // Assert
            Assert.AreEqual(string.Empty, exchangeName);
        }
    }
}
// ReSharper restore InconsistentNaming