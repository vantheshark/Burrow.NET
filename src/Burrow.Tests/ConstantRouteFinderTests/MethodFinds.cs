using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Burrow.Tests.ConstantRouteFinderTests
{
    [TestClass]
    public class MethodFinds
    {
        [TestMethod]
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
