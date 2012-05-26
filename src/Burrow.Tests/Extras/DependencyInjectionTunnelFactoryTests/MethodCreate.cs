using Burrow.Extras;
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
            Assert.IsInstanceOfType(tunnel, typeof(RabbitTunnel));
        }
    }
}
// ReSharper restore InconsistentNaming