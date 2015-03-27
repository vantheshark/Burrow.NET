using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultRpcRouteFinderTests
{
    [TestFixture]
    public class PropertyRequestExchangeType
    {
        [Test]
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