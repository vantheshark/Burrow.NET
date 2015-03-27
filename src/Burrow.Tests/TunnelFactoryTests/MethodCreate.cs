using System;
using System.Reflection;
using Burrow.Internal;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.TunnelFactoryTests
{
    [TestFixture]
    public class MethodCreate
    {
        [Test, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_cannot_find_RMQ_connection_string()
        {
            RabbitTunnel.Factory = new TunnelFactory();
            RabbitTunnel.Factory.Create();
        }


        [Test]
        public void Should_be_able_to_create_tunnel_with_provided_values()
        {
            // Arrange
            FieldInfo fi = typeof(RabbitTunnel).GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
            RabbitTunnel.Factory = new TunnelFactory();

            // Action
            var tunnel = RabbitTunnel.Factory.Create("localhost", 1000, "/", "guest", "guest", NSubstitute.Substitute.For<IRabbitWatcher>());

            // Assert
            Assert.IsNotNull(fi);
            Assert.IsTrue(fi.GetValue(tunnel) is DurableConnection);
        }


        [Test]
        public void Should_create_ha_connection_if_provide_cuslter_connection_string()
        {
            // Arrange
            FieldInfo fi = typeof(RabbitTunnel).GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);
            RabbitTunnel.Factory = new TunnelFactory();

            // Action
            var tunnel = RabbitTunnel.Factory.Create("host=unreachable1.com;username=guest;password=guest|host=unreachable2.com;username=guest;password=guest|host=unreachable3.com;username=guest;password=guest");
            
            // Assert
            Assert.IsNotNull(fi);
            Assert.IsTrue(fi.GetValue(tunnel) is HaConnection);
        }
    }
}
// ReSharper restore InconsistentNaming