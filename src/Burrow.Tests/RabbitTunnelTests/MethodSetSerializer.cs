using System;

using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestFixture]
    public class MethodSetSerializer
    {
        [Test]
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