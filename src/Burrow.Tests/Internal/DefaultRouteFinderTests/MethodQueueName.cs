using Burrow.Internal;using NUnit.Framework;
using Burrow.Tests.Extras.RabbitSetupTests;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DefaultRouteFinderTests
{
    [TestFixture]
    public class MethodQueueName
    {
        [Test]
        public void Should_return_burrow_queue_name()
        {
            // Arrange
            var routeFinder = new DefaultRouteFinder();

            // Action
            var keyName = routeFinder.FindQueueName<Customer>(null);

            // Assret
            Assert.AreEqual("Burrow.Queue.Customer", keyName);
        }

        [Test]
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