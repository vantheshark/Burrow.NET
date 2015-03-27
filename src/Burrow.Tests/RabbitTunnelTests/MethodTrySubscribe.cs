using System;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestFixture]
    public class MethodTrySubscribe
    {
        [Test]
        public void Should_not_throw_OperationInterruptedException()
        {
            // Arrange
            IDurableConnection durableConnection;
            var tunnel = (RabbitTunnelForTest)RabbitTunnelForTest.CreateTunnel(null, out durableConnection);

            // Action
            tunnel.TrySubscribeForTest(() => { throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 3,"")); });
        }

        [Test]
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