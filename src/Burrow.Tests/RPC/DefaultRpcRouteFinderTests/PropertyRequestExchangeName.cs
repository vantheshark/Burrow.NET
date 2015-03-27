using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultRpcRouteFinderTests
{
    [TestFixture]
    public class PropertyRequestExchangeName
    {
        [Test]
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