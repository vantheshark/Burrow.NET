using System.Reflection;
using Burrow.Extras;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.DependencyInjectionTunnelFactoryTests
{
    [TestClass]
    public class MethodCreate
    {
        [TestMethod]
        public void Should_use_resolver_to_resolve_tunnel()
        {
            // Arrange
            var resolver = Substitute.For<IBurrowResolver>();
            resolver.Resolve<ITunnel>().Returns(Substitute.For<ITunnel>());
            RabbitTunnel.Factory.RegisterResolver(resolver);

            // Action
            var tunnel = RabbitTunnel.Factory.Create();

            // Assert
            Assert.IsTrue(RabbitTunnel.Factory is DependencyInjectionTunnelFactory);
            Assert.IsNotNull(tunnel);
        }

        [TestMethod]
        public void Should_use_default_implementation_if_cannot_resolve_objects()
        {
            // Arrange
            var resolver = Substitute.For<IBurrowResolver>();
            resolver.Resolve<IConsumerManager>().Returns((IConsumerManager)null);
            resolver.Resolve<IMessageHandlerFactory>().Returns((IMessageHandlerFactory)null);
            resolver.Resolve<IConsumerErrorHandler>().Returns((IConsumerErrorHandler)null);
            resolver.Resolve<IRetryPolicy>().Returns((IRetryPolicy)null);

            RabbitTunnel.Factory.RegisterResolver(resolver);

            // Action
            var tunnel = RabbitTunnel.Factory.Create("");

            // Assert
            Assert.IsTrue(RabbitTunnel.Factory is DependencyInjectionTunnelFactory);
            Assert.IsInstanceOfType(tunnel, typeof(RabbitTunnel));
        }


        [TestMethod]
        public void Should_create_tunnel_with_HaConnection_if_the_connectionString_says_so()
        {
            // Arrange
            FieldInfo fi = typeof(RabbitTunnel).GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
            var resolver = Substitute.For<IBurrowResolver>();
            resolver.Resolve<IConsumerManager>().Returns((IConsumerManager)null);
            resolver.Resolve<IMessageHandlerFactory>().Returns((IMessageHandlerFactory)null);
            resolver.Resolve<IConsumerErrorHandler>().Returns((IConsumerErrorHandler)null);
            resolver.Resolve<IRetryPolicy>().Returns((IRetryPolicy)null);

            RabbitTunnel.Factory.RegisterResolver(resolver);

            // Action
            var tunnel = RabbitTunnel.Factory.Create("host=rabbitmq.com:5672;username=guest;password=guest|host=2nd.rabbitmq.com:5672;username=guest;password=guest", Substitute.For<IRabbitWatcher>());

            // Assert
            Assert.IsTrue(RabbitTunnel.Factory is DependencyInjectionTunnelFactory);
            Assert.IsInstanceOfType(tunnel, typeof(RabbitTunnel));
            Assert.IsTrue(fi.GetValue(tunnel) is HaConnection);
        }
    }
}
// ReSharper restore InconsistentNaming