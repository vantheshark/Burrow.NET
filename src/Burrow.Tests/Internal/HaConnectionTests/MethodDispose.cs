using System.Collections.Generic;
using Burrow.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.HaConnectionTests
{
    [TestFixture]
    public class MethodDispose 
    {
        [Test]
        public void Should_unsubscribe_to_ConnectionEstablished_event()
        {
            var connectionFactory = new ManagedConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                VirtualHost = "/",
                UserName = "vantheshark",
                Password = "123"
            };
            var connection = new HaConnection(Substitute.For<IRetryPolicy>(),
                             Substitute.For<IRabbitWatcher>(),
                             new List<ManagedConnectionFactory>
                             {
                                 connectionFactory
                             });

            var fired = false;
            connection.Connected += () => { fired = true; };
            ManagedConnectionFactory.ConnectionEstablished += (a, b) => { }; //NOTE: To make it not null
            connection.Dispose();

            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var c = Substitute.For<IConnection>();
            c.CreateModel().Returns(model);
            c.IsOpen.Returns(true);
            c.Endpoint.Returns(connectionFactory.Endpoint);
            ManagedConnectionFactory.SharedConnections[connectionFactory.Endpoint + connectionFactory.VirtualHost] = c;

            ManagedConnectionFactory.ConnectionEstablished.Invoke(new AmqpTcpEndpoint("localhost"), "/");

            Assert.IsFalse(fired);
        }
    }
}
// ReSharper restore InconsistentNaming