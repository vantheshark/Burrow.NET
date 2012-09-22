using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultRpcRouteFinderTests
{
    [TestClass]
    public class PropertyRequestExchangeName
    {
        [TestMethod]
        public void Should_always_return_empty_string()
        {
            // Arrange
            var routeFinder = new DefaultRpcRouteFinder<ISomeService>();

            // Action
            var exchangeName = routeFinder.RequestExchangeName;

            // Assert
            Assert.AreEqual(string.Empty, exchangeName);
        }
    }
}
// ReSharper restore InconsistentNaming