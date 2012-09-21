using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodSetSerializer
    {
        [TestMethod]
        public void Should_notify_all_registered_observers()
        {
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);

            var observer = Substitute.For<IObserver<ISerializer>>();
            var serializer = Substitute.For<ISerializer>();

            tunnel.AddSerializerObserver(observer);

            // Action
            tunnel.SetSerializer(serializer);

            // Assert
            observer.Received(1).OnNext(serializer);

        }
    }
}
// ReSharper restore InconsistentNaming