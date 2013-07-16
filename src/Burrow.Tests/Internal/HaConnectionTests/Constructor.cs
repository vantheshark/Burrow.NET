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




    }
}
// ReSharper restore InconsistentNaming