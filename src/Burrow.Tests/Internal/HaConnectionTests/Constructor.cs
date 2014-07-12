using System.Collections.Generic;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.HaConnectionTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod]
        public void Should_convert_connection_factories_to_managed_connection_factories()
        {
            // Arrange
            var connection = new HaConnection(Substitute.For<IRetryPolicy>(), 
                             Substitute.For<IRabbitWatcher>(), 
                             new List<ManagedConnectionFactory>
                             {
                                 new ManagedConnectionFactory
                                 {
                                     HostName = "localhost",
                                     Port = 5672,
                                     VirtualHost = "/",
                                     UserName = "vantheshark",
                                     Password = "123"
                                 }
                             });

            Assert.IsTrue(connection.ConnectionFactory is ManagedConnectionFactory);
            Assert.AreEqual("localhost", connection.ConnectionFactory.HostName);
            Assert.AreEqual("vantheshark", connection.ConnectionFactory.UserName);
            Assert.AreEqual("123", connection.ConnectionFactory.Password);
            Assert.AreEqual(5672, connection.ConnectionFactory.Port);
            Assert.AreEqual("/", connection.ConnectionFactory.VirtualHost);
        }


        [TestMethod]
        public void Should_bind_to_ConnectionEstablished_event_and_change_connection_status_if_a_share_connection_established()
        {
            var connection = Substitute.For<IConnection>();
            connection.IsOpen.Returns(true);
            var factory1 = Substitute.For<ManagedConnectionFactory>();
            factory1.HostName = "host1";
            factory1.VirtualHost = "/virtualhost";
            factory1.EstablishConnection().Returns(connection);


            var factory2 = Substitute.For<ManagedConnectionFactory>();
            factory2.HostName = "host2";
            factory2.VirtualHost = "/virtualhost";

            var factory3 = Substitute.For<ManagedConnectionFactory>();
            factory2.HostName = "host3";
            factory2.VirtualHost = "/virtualhost";

            var haConnection = new HaConnection(Substitute.For<IRetryPolicy>(),
                              Substitute.For<IRabbitWatcher>(),
                              new List<ManagedConnectionFactory> { factory3, factory2, factory1 });


            Assert.AreEqual(factory3.HostName, haConnection.ConnectionFactories.Current.HostName);
            Assert.AreEqual(factory3.VirtualHost, haConnection.ConnectionFactories.Current.VirtualHost);

            // Action
            factory1.CreateConnection();

            // Assert
            Assert.AreEqual(factory1.HostName, haConnection.ConnectionFactories.Current.HostName);
            Assert.AreEqual(factory1.VirtualHost, haConnection.ConnectionFactories.Current.VirtualHost);
        }
    }
}
// ReSharper restore InconsistentNaming