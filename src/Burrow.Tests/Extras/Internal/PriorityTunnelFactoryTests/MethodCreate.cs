using Burrow.Extras.Internal;
using Burrow.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityTunnelFactoryTests
{
    [TestClass]
    public class MethodCreate
    {
        [TestMethod]
        public void Should_return_RabbitTunnelWithPriorityQueuesSupport_object()
        {
            // Arrange
            var factory = new PriorityTunnelFactory();

            // Action
            var tunnel = factory.Create("");

            // Assert
            Assert.IsInstanceOfType(tunnel, typeof(RabbitTunnelWithPriorityQueuesSupport));
        }

        [TestMethod]
        public void Should_use_DependencyInjectionTunnelFactory_to_create_tunnel_if_it_is_default_TunnelFactory()
        {
            // Arrange
            var factory = new PriorityTunnelFactory();
            RabbitTunnel.Factory.RegisterResolver(NSubstitute.Substitute.For<IBurrowResolver>());

            // Action
            var tunnel = factory.Create("");

            // Assert
            Assert.IsInstanceOfType(tunnel, typeof(RabbitTunnel));
        }
    }
}
// ReSharper restore InconsistentNaming