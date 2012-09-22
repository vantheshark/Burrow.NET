using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultFanoutRpcRequestRouteFinderTests
{
    [TestClass]
    public class PropertyRequestExchangeType
    {
        [TestMethod]
        public void Should_always_be_fanout()
        {
            // Arrange
            var routeFinder = new DefaultFanoutRpcRequestRouteFinder<ISomeService>();

            // Action
            var exchangeType = routeFinder.RequestExchangeType;

            // Assert
            Assert.AreEqual("fanout", exchangeType);
        }
    }
}
// ReSharper restore InconsistentNaming