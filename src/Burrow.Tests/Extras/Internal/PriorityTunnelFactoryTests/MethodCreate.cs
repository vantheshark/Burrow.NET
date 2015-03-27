using System.Reflection;
using Burrow.Extras;
using Burrow.Extras.Internal;
using Burrow.Internal;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityTunnelFactoryTests
{
    [TestFixture]
    public class MethodCreate
    {
        [Test]
        public void Should_return_RabbitTunnelWithPriorityQueuesSupport_object()
        {
            // Arrange
            new TunnelFactory(); // To reset default factory
            var factory = new PriorityTunnelFactory();

            // Action
            var tunnel = factory.Create("");

            // Assert
            Assert.IsInstanceOfType(typeof(RabbitTunnelWithPriorityQueuesSupport), tunnel);
        }

        [Test]
        public void Should_use_DependencyInjectionTunnelFactory_to_create_tunnel_if_it_is_default_TunnelFactory()
        {
            // Arrange
            var bak = RabbitTunnel.Factory;
            var factory = new PriorityTunnelFactory();
            RabbitTunnel.Factory.RegisterResolver(Substitute.For<IBurrowResolver>());

            // Action
            var tunnel1 = factory.Create("");
            var tunnel2 = factory.Create("hostname", 5672, "/", "user", "pass", null);

            // Assert
            Assert.IsTrue(RabbitTunnel.Factory is DependencyInjectionTunnelFactory);
            Assert.IsInstanceOfType(typeof(RabbitTunnel), tunnel1);
            Assert.IsInstanceOfType(typeof(RabbitTunnel), tunnel2);
            RabbitTunnel.Factory = bak;
        }

        [Test]
        public void Should_return_a_tunnel_using_PriorityMessageHandlerFactory()
        {
            // Arrange
            new TunnelFactory(); // To reset default factory
            var factory = new PriorityTunnelFactory();

            // Action
            var tunnel = factory.Create("");
            var consumerManager = (IConsumerManager)typeof(RabbitTunnel).GetField("_consumerManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tunnel);

            // Assert
            Assert.IsInstanceOfType(typeof(PriorityMessageHandlerFactory), consumerManager.MessageHandlerFactory);
        }


        [Test]
        public void Should_create_ha_connection_if_provide_cuslter_connection_string()
        {
            // Arrange
            FieldInfo fi = typeof(RabbitTunnelWithPriorityQueuesSupport).GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
            var factory = new PriorityTunnelFactory();

            // Action
            var tunnel = factory.Create("host=unreachable1.com;username=guest;password=guest|host=unreachable2.com;username=guest;password=guest|host=unreachable3.com;username=guest;password=guest");

            // Assert
            Assert.IsNotNull(fi);
            Assert.IsTrue(fi.GetValue(tunnel) is HaConnection);
        }
    }
}
// ReSharper restore InconsistentNaming