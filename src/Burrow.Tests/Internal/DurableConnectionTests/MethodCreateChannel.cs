using System;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestClass]
    public class MethodCreateChannel : DurableConnectionTestHelper
    {
        [TestMethod]
        public void Should_try_to_connection_first_if_not_connected()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost17", out rmqConnection);
            var channel = Substitute.For<IModel>();
            rmqConnection.CreateModel().Returns(channel);

            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);

            // Action
            durableConnection.CreateChannel();

            //Assert
            connectionFactory.Received().CreateConnection();
        }

        [TestMethod, ExpectedException(typeof(BrokerUnreachableException))]
        public void Should_throw_BrokerUnreachableException_if_cannot_connect_during_creating_channel()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost18", out rmqConnection);
            rmqConnection.IsOpen.Returns(false);

            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);

            // Action
            durableConnection.CreateChannel();
        }
    }
}
// ReSharper restore InconsistentNaming