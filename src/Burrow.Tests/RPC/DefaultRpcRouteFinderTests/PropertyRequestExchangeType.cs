using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultRpcRouteFinderTests
{
    [TestClass]
    public class PropertyRequestExchangeType
    {
        [TestMethod]
        public void Should_return_null()
        {
            // Arrange
            var routeFinder = new DefaultRpcRouteFinder<ISomeService>("UnitTest");

            // Action
            var type = routeFinder.RequestExchangeType;

            // Assert
            Assert.IsNull(type);
        }
    }
}
// ReSharper restore InconsistentNaming