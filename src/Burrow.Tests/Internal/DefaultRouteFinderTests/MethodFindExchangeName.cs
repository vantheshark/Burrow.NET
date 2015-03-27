using Burrow.Internal;using NUnit.Framework;
using Burrow.Tests.Extras.RabbitSetupTests;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DefaultRouteFinderTests
{
    [TestFixture]
    public class MethodFindExchangeName
    {
        [Test]
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