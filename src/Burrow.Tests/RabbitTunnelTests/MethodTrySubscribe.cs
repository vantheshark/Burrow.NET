using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodTrySubscribe
    {
        [TestMethod]
        public void Should_not_throw_OperationInterruptedException()
        {
            // Arrange
            IDurableConnection durableConnection;
            var tunnel = (RabbitTunnelForTest)RabbitTunnelForTest.CreateTunnel(null, out durableConnection);

            // Action
            tunnel.TrySubscribeForTest(() => { throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 3,"")); });
        }

        [TestMethod]
        public void Should_not_throw_any_other_exceptions()
        {
            // Arrange
            IDurableConnection durableConnection;
            var tunnel = (RabbitTunnelForTest)RabbitTunnelForTest.CreateTunnel(null, out durableConnection);

            // Action
            tunnel.TrySubscribeForTest(() => { throw new Exception("Test Exception"); });
        }
    }
}
// ReSharper restore InconsistentNaming