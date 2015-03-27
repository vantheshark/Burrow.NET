using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultFanoutRpcRequestRouteFinderTests
{
    [TestFixture]
    public class PropertyRequestExchangeName
    {
        [Test]
        public void Should_return_not_empty_string()
        {
            // Arrange
            var routeFinder = new DefaultFanoutRpcRequestRouteFinder<ISomeService>();

            // Action
            var exchangeName = routeFinder.RequestExchangeName;

            // Assert
            Assert.AreEqual("Burrow.Exchange.FANOUT.Rpc.ISomeService.Requests", exchangeName);
        }
    }
}
// ReSharper restore InconsistentNaming