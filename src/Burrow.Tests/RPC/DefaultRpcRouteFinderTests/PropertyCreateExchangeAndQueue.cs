using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultRpcRouteFinderTests
{
    [TestClass]
    public class PropertyCreateExchangeAndQueue
    {
        [TestMethod]
        public void Should_return_true()
        {
            // Arrange
            var routeFinder = new DefaultRpcRouteFinder<ISomeService>("UnitTest");

            // Action
            var create = routeFinder.CreateExchangeAndQueue;

            // Assert
            Assert.IsTrue(create);
        }
    }
}
// ReSharper restore InconsistentNaming