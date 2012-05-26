using Burrow.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DefaultRouteFinderTests
{
    [TestClass]
    public class MethodQueueName
    {
        [TestMethod]
        public void Should_return_burrow_queue_name()
        {
            // Arrange
            var routeFinder = new DefaultRouteFinder();

            // Action
            var keyName = routeFinder.FindQueueName<Customer>(null);

            // Assret
            Assert.AreEqual("Burrow.Queue.Customer", keyName);
        }

        [TestMethod]
        public void Should_return_burrow_queue_name_with_subscription_if_provided()
        {
            // Arrange
            var routeFinder = new DefaultRouteFinder();

            // Action
            var keyName = routeFinder.FindQueueName<Customer>("TESTAPP");

            // Assret
            Assert.AreEqual("Burrow.Queue.TESTAPP.Customer", keyName);
        }
    }
}
// ReSharper restore InconsistentNaming