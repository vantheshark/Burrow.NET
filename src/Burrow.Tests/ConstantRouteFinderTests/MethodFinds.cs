using Burrow.Tests.Extras.RabbitSetupTests;
using NUnit.Framework;


namespace Burrow.Tests.ConstantRouteFinderTests
{
    [TestFixture]
    public class MethodFinds
    {
        [Test]
// ReSharper disable InconsistentNaming
        public void Should_return_provided_exchange_name()
// ReSharper restore InconsistentNaming
        {
            // Arrange & Action
            var finder = new ConstantRouteFinder("e", "q", "r");

            // Assert
            Assert.AreEqual("e", finder.FindExchangeName<Customer>());
            Assert.AreEqual("q", finder.FindQueueName<Customer>("s"));
            Assert.AreEqual("r", finder.FindRoutingKey<Customer>());
        }
    }
}
