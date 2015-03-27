using Burrow.Internal;using NUnit.Framework;
using Burrow.Tests.Extras.RabbitSetupTests;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DefaultRouteFinderTests
{
    [TestFixture]
    public class MethodFindRoutingKey
    {
        [Test]
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