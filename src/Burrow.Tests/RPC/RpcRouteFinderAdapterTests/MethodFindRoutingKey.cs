using System;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcRouteFinderAdapterTests
{
    [TestClass]
    public class MethodFindRoutingKey
    {
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void Should_always_throw_exception()
        {
            // Arrange
            var routeFinder = new RpcRouteFinderAdapter(NSubstitute.Substitute.For<IRpcRouteFinder>());

            // Action
            routeFinder.FindRoutingKey<RpcRequest>();
        }
    }
}
// ReSharper restore InconsistentNaming