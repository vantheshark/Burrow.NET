using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultFanoutRpcRequestRouteFinderTests
{
    [TestClass]
    public class PropertyRequestExchangeName
    {
        [TestMethod]
        public void Should_return_not_empty_string()
        {
            // Arrange
            var routeFinder = new DefaultFanoutRpcRequestRouteFinder<ISomeService>();

            // Action
            var exchangeName = routeFinder.RequestExchangeName;

            // Assert
            Assert.AreEqual("Burrow.Exchange.Rpc.ISomeService.Requests", exchangeName);
        }
    }
}
// ReSharper restore InconsistentNaming