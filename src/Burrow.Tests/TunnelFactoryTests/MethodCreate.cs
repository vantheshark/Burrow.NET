using System;
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
    }
}
// ReSharper restore InconsistentNaming