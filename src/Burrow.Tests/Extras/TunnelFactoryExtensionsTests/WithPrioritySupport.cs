using System;
using Burrow.Extras;
using Burrow.Extras.Internal;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.TunnelFactoryExtensionsTests
{
    [TestFixture]
    public class WithPrioritySupport
    {
        [SetUp]
        public void InitTests()
        {
            RabbitTunnel.Factory = new TunnelFactory();
        }

        [Test]
        public void Should_return_PriorityTunnelFactory()
        {
            // Arrange & Action
            var tunnelFactory = RabbitTunnel.Factory.WithPrioritySupport();

            // Assert
            Assert.IsInstanceOfType(typeof(PriorityTunnelFactory), tunnelFactory);
        }

        [Test]
        public void Should_return_same_instance_if_already_a_PriorityTunnelFactory()
        {
            // Arrange & Action
            var factory = new PriorityTunnelFactory();
            var tunnelFactory = factory.WithPrioritySupport();

            // Assert
            Assert.AreSame(tunnelFactory, factory);
        }


        [Test, ExpectedException(ExpectedException = typeof(InvalidCastException), ExpectedMessage = "Current tunnel object is not supporting priority queues")]
        public void Should_throw_exception_if_current_factory_is_not_PriorityTunnelFactory()
        {
            // Arrange
            var tunnel = RabbitTunnel.Factory.Create("");

            // Action
            tunnel.WithPrioritySupport();
        }

        [Test]
        public void Should_return_ITunnelWithPrioritySupport()
        {
            // Arrange
            var tunnelFactory = RabbitTunnel.Factory.WithPrioritySupport();
            var tunnel = tunnelFactory.Create("");
            
            // Action
            tunnel = tunnel.WithPrioritySupport();

            // Assert
            Assert.IsInstanceOfType(typeof(ITunnelWithPrioritySupport), tunnel);
        }
    }
}
// ReSharper restore InconsistentNaming