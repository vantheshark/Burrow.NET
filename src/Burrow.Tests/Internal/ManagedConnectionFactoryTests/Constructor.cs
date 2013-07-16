using System;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ManagedConnectionFactoryTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod]
        public void Should_copy_all_value_from_privided_factory()
        {
            // Arrange
            var originalFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                VirtualHost = "/",
                UserName = "vantheshark",
                Password = "123"
            };

            // Action
            var factory = new ManagedConnectionFactory(originalFactory);


            // Assert
            Assert.AreEqual(JsonConvert.SerializeObject(originalFactory), JsonConvert.SerializeObject(factory));
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provide_null_connectionString()
        {
            ConnectionString cnn = null;
            new ManagedConnectionFactory(cnn);
        }
    }
}
// ReSharper restore InconsistentNaming