using Burrow.Internal;
using Burrow.Tests.Internal.DurableConnectionTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ManagedConnectionFactoryTests
{
    [TestClass]
    public class MethodCreateConnection : DurableConnectionTestHelper
    {
        [TestMethod]
        public void Should_save_created_connection()
        {
            // Arrange
            var connection = Substitute.For<IConnection>();
            connection.IsOpen.Returns(true);
            var factory = Substitute.For<ManagedConnectionFactory>();
            factory.HostName = "localhost";
            factory.VirtualHost = "/virtualhost";
            factory.CreateConnection().Returns(connection)
                                      .AndDoes(callInfo => factory.SaveConnection(connection));

            // Action
            factory.CreateConnection();
            
            // Assert
            Assert.AreEqual(1, ManagedConnectionFactory.SharedConnections.Count);
        }


        [TestMethod]
        public void Should_clear_existing_connection_from_shared_connection_list_if_connection_is_dropped_by_peer()
        {
            var connection = Substitute.For<IConnection>();
            connection.IsOpen.Returns(true);
            var factory = Substitute.For<ManagedConnectionFactory>();
            factory.HostName = "localhost";
            factory.VirtualHost = "/virtualhost";
            factory.CreateConnection().Returns(connection)
                                      .AndDoes(callInfo => factory.SaveConnection(connection));

            // Action
            factory.CreateConnection();
            Assert.AreEqual(1, ManagedConnectionFactory.SharedConnections.Count);
            connection.ConnectionShutdown += Raise.Event<ConnectionShutdownEventHandler>(connection, new ShutdownEventArgs(ShutdownInitiator.Application, 0, "Connection dropped for unknow reason ;)"));
            // Assert
            Assert.AreEqual(0, ManagedConnectionFactory.SharedConnections.Count);
        }
    }
}
// ReSharper restore InconsistentNaming