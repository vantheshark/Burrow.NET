using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultFanoutRpcRequestRouteFinderTests
{
    [TestFixture]
    public class PropertyRequestExchangeType
    {
        [Test]
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