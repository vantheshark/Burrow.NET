using System;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestClass]
    public class MethodSharedConnectionShutdown : DurableConnectionTestHelper
    {
        [TestMethod]
        public void Should_be_call_if_App_is_closed_by_user()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory("/", out rmqConnection);
            
            
            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            durableConnection.Disconnected += () => { };
            durableConnection.Connect();

            // Action
            rmqConnection.ConnectionShutdown += Raise.Event<ConnectionShutdownEventHandler>(rmqConnection, new ShutdownEventArgs(ShutdownInitiator.Application, 0, "Connection disposed by application"));
            
            //Assert
            retryPolicy.DidNotReceive().WaitForNextRetry(Arg.Any<Action>());
        }

        [TestMethod]
        public void Should_retry_if_connection_is_dropped_by_peer()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory("/", out rmqConnection);


            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            durableConnection.Disconnected += () => { };
            durableConnection.Connect();

            // Action
            rmqConnection.ConnectionShutdown += Raise.Event<ConnectionShutdownEventHandler>(rmqConnection, new ShutdownEventArgs(ShutdownInitiator.Application, 0, "Connection dropped for unknow reason ;)"));

            //Assert
            retryPolicy.Received().WaitForNextRetry(Arg.Any<Action>());
        }
    }
}
// ReSharper restore InconsistentNaming