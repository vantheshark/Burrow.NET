using Burrow.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DefaultRouteFinderTests
{
    [TestClass]
    public class MethodFindRoutingKey
    {
        [TestMethod]
        public void Should_return_name_of_message_type()
        {
            // Arrange
            var routeFinder = new DefaultRouteFinder();

            // Action
            var keyName = routeFinder.FindRoutingKey<Customer>();

            // Assret
            Assert.AreEqual("Customer", keyName);
        }
    }
}
// ReSharper restore InconsistentNaming