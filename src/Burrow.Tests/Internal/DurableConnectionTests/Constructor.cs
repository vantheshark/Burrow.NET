using System;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_retryPolicy_is_null()
        {
            new DurableConnection(null, Substitute.For<IRabbitWatcher>(), Substitute.For<ConnectionFactory>());
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_watcher_is_null()
        {
            new DurableConnection(Substitute.For<IRetryPolicy>(), null, Substitute.For<ConnectionFactory>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_ConnectionFactory_is_null()
        {
            new DurableConnection(Substitute.For<IRetryPolicy>(), Substitute.For<IRabbitWatcher>(), null);
        }

        [TestMethod]
        public void Should_convert_connection_factories_to_managed_connection_factories()
        {
            var connection = new DurableConnection(Substitute.For<IRetryPolicy>(),
                             Substitute.For<IRabbitWatcher>(), new ConnectionFactory {
                                HostName = "localhost",
                                UserName = "vantheshark",
                                Password = "123"
                             });

            Assert.IsTrue(connection.ConnectionFactory is ManagedConnectionFactory);
            Assert.AreEqual("localhost", connection.ConnectionFactory.HostName);
            Assert.AreEqual("vantheshark", connection.ConnectionFactory.UserName);
            Assert.AreEqual("123", connection.ConnectionFactory.Password);
        }
    }
}
// ReSharper restore InconsistentNaming