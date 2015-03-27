using Burrow.Internal;
using Burrow.Tests.Internal.DurableConnectionTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ManagedConnectionFactoryTests
{
    [TestFixture]
    public class MethodCreateConnection : DurableConnectionTestHelper
    {
        [Test]
        public void Should_save_created_connection()
        {
            // Arrange
            var connection = Substitute.For<IConnection>();
            connection.IsOpen.Returns(true);
            var factory = Substitute.For<ManagedConnectionFactory>();
            factory.HostName = "localhost";
            factory.VirtualHost = "/virtualhost";
            factory.EstablishConnection().Returns(connection);

            // Action
            factory.CreateConnection();
            
            // Assert
            Assert.AreEqual(1, ManagedConnectionFactory.SharedConnections.Count);
        }

        [Test]
        public void Should_clear_existing_connection_from_shared_connection_list_if_connection_is_dropped_by_peer()
        {
            ManagedConnectionFactory.SharedConnections.Clear();
            var connection = Substitute.For<IConnection>();
            connection.IsOpen.Returns(true);
            
            var factory = Substitute.For<ManagedConnectionFactory>();
            factory.HostName = "localhost";
            factory.VirtualHost = "/virtualhost";
            factory.EstablishConnection().Returns(connection);

            // Action
            factory.CreateConnection();
            Assert.AreEqual(1, ManagedConnectionFactory.SharedConnections.Count);
            connection.ConnectionShutdown += Raise.Event<ConnectionShutdownEventHandler>(connection, new ShutdownEventArgs(ShutdownInitiator.Application, 0, "Connection dropped for unknow reason ;)"));
            // Assert
            Assert.AreEqual(0, ManagedConnectionFactory.SharedConnections.Count);
        }

        [Test]
        public void Should_fire_event_when_new_connection_established()
        {
            var connectedEndpoint = string.Empty;
            var connection1 = Substitute.For<IConnection>();
            connection1.IsOpen.Returns(true);

            var factory1 = Substitute.For<ManagedConnectionFactory>();
            factory1.HostName = "localhost";
            factory1.VirtualHost = "/virtualhost";
            factory1.EstablishConnection().Returns(connection1);

            ManagedConnectionFactory.ConnectionEstablished += (a, b) => { connectedEndpoint = a + b; };

            // Action
            factory1.CreateConnection();
            Assert.AreEqual("amqp://localhost:5672/virtualhost", connectedEndpoint);
        }
    }
}
// ReSharper restore InconsistentNaming