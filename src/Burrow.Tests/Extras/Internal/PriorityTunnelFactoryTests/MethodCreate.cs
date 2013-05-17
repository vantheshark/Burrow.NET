using System.Reflection;
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
            var bak = RabbitTunnel.Factory;
            var factory = new PriorityTunnelFactory();
            RabbitTunnel.Factory.RegisterResolver(NSubstitute.Substitute.For<IBurrowResolver>());

            // Action
            var tunnel = factory.Create("");

            // Assert
            Assert.IsInstanceOfType(tunnel, typeof(RabbitTunnel));
            RabbitTunnel.Factory = bak;
        }

        [TestMethod]
        public void Should_return_a_tunnel_using_PriorityMessageHandlerFactory()
        {
            // Arrange
            var factory = new PriorityTunnelFactory();

            // Action
            var tunnel = factory.Create("");
            var consumerManager = (IConsumerManager)typeof(RabbitTunnel).GetField("_consumerManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tunnel);

            // Assert
            Assert.IsInstanceOfType(consumerManager.MessageHandlerFactory, typeof(PriorityMessageHandlerFactory));
        }
    }
}
// ReSharper restore InconsistentNaming