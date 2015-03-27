using System;
using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcRouteFinderAdapterTests
{
    [TestFixture]
    public class MethodFindRoutingKey
    {
        [Test, ExpectedException(typeof(NotSupportedException))]
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