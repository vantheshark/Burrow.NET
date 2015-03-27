using System;
using Burrow.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestFixture]
    public class Constructor
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_retryPolicy_is_null()
        {
            new DurableConnection(null, Substitute.For<IRabbitWatcher>(), Substitute.For<ConnectionFactory>());
        }


        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_watcher_is_null()
        {
            new DurableConnection(Substitute.For<IRetryPolicy>(), null, Substitute.For<ConnectionFactory>());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_ConnectionFactory_is_null()
        {
            new DurableConnection(Substitute.For<IRetryPolicy>(), Substitute.For<IRabbitWatcher>(), null);
        }

        [Test]
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

        [Test]
        public void Should_subscribe_to_ConnectionEstablished_event()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "vantheshark",
                Password = "123"
            };
            using(var connection = new DurableConnection(Substitute.For<IRetryPolicy>(), Substitute.For<IRabbitWatcher>(), connectionFactory))
            {
                var fired = false;
                connection.Connected += () => { fired = true; };
                ManagedConnectionFactory.ConnectionEstablished += (a, b) => { }; //NOTE: To make it not null

                var model = Substitute.For<IModel>();
                model.IsOpen.Returns(true);
                var c = Substitute.For<IConnection>();
                c.CreateModel().Returns(model);
                c.IsOpen.Returns(true);
                c.Endpoint.Returns(connectionFactory.Endpoint);

                ManagedConnectionFactory.SharedConnections[connectionFactory.Endpoint + connectionFactory.VirtualHost] = c;
                ManagedConnectionFactory.ConnectionEstablished.Invoke(new AmqpTcpEndpoint("localhost"), "/");

                Assert.IsTrue(fired);
            }
        }
    }
}
// ReSharper restore InconsistentNaming