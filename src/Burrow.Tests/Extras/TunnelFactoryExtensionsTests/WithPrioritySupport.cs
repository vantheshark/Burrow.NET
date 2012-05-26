using System;
using Burrow.Extras;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.TunnelFactoryExtensionsTests
{
    [TestClass]
    public class WithPrioritySupport
    {
        [TestInitialize]
        public void InitTests()
        {
            RabbitTunnel.Factory = new TunnelFactory();
        }

        [TestMethod]
        public void Should_return_PriorityTunnelFactory()
        {
            // Arrange & Action
            var tunnelFactory = RabbitTunnel.Factory.WithPrioritySupport();

            // Assert
            Assert.IsInstanceOfType(tunnelFactory, typeof(PriorityTunnelFactory));
        }


        [TestMethod, ExpectedException(typeof(InvalidCastException), "Current tunnel object is supporting priority queues")]
        public void Should_throw_exception_if_current_factory_is_not_PriorityTunnelFactory()
        {
            // Arrange
            var tunnel = RabbitTunnel.Factory.Create("");

            // Action
            tunnel.WithPrioritySupport();
        }

        [TestMethod]
        public void Should_return_ITunnelWithPrioritySupport()
        {
            // Arrange
            var tunnelFactory = RabbitTunnel.Factory.WithPrioritySupport();
            var tunnel = tunnelFactory.Create("");
            
            // Action
            tunnel = tunnel.WithPrioritySupport();

            // Assert
            Assert.IsInstanceOfType(tunnel, typeof(ITunnelWithPrioritySupport));
        }
    }
}
// ReSharper restore InconsistentNaming