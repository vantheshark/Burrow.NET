using Burrow.RPC;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultRpcRouteFinderTests
{
    [TestFixture]
    public class PropertyCreateExchangeAndQueue
    {
        [Test]
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