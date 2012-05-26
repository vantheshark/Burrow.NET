using Burrow.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DefaultRouteFinderTests
{
    [TestClass]
    public class MethodFindExchangeName
    {
        [TestMethod]
        public void Should_return_BurrowExchange()
        {
            // Arrange
            var routeFinder = new DefaultRouteFinder();

            // Action
            var exchangeName = routeFinder.FindExchangeName<Customer>();

            // Assret
            Assert.AreEqual("Burrow.Exchange", exchangeName);
        }
    }
}
// ReSharper restore InconsistentNaming