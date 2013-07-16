using System;
using System.Reflection;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.TunnelFactoryTests
{
    [TestClass]
    public class MethodCreate
    {
        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_cannot_find_RMQ_connection_string()
        {
            RabbitTunnel.Factory = new TunnelFactory();
            RabbitTunnel.Factory.Create();
        }

        [TestMethod]
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