using Burrow.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestFixture]
    public class MethodDispose : DurableConnectionTestHelper
    {
        [Test]
        public void Should_NOT_close_any_connection()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost19", out rmqConnection);

            var channel = Substitute.For<IModel>();
            rmqConnection.CreateModel().Returns(channel);

            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            var count = ManagedConnectionFactory.SharedConnections.Count;
            durableConnection.Connect();
            durableConnection.CreateChannel();
            Assert.AreEqual(count + 1, ManagedConnectionFactory.SharedConnections.Count);


            // Action
            durableConnection.Dispose();

            //Assert
            rmqConnection.DidNotReceive().Close(Arg.Any<ushort>(), Arg.Any<string>());
            rmqConnection.DidNotReceive().Dispose();
            Assert.AreEqual(count + 1, ManagedConnectionFactory.SharedConnections.Count);
        }


        [Test]
        public void Should_unsubscribe_to_ConnectionEstablished_event()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "vantheshark",
                Password = "123"
            };
            var connection = new DurableConnection(Substitute.For<IRetryPolicy>(), Substitute.For<IRabbitWatcher>(), connectionFactory);

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