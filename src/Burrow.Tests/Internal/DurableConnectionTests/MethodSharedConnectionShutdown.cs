using System;
using Burrow.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestFixture]
    public class MethodSharedConnectionShutdown : DurableConnectionTestHelper
    {
        [Test]
        public void Should_be_called_if_App_is_closed_by_user()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost20", out rmqConnection);
            
            
            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            durableConnection.Disconnected += () => { };
            durableConnection.Connect();

            // Action
            rmqConnection.ConnectionShutdown += Raise.EventWith(rmqConnection, new ShutdownEventArgs(ShutdownInitiator.Application, 0, "Connection disposed by application"));
            
            //Assert
            retryPolicy.DidNotReceive().WaitForNextRetry(Arg.Any<Action>());
        }

        [Test]
        public void Should_try_reconnect_by_retryPolicy_if_Connection_Shutdown_event_was_fired()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost21", out rmqConnection);


            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            durableConnection.Disconnected += () => { };
            durableConnection.Connect();
            Assert.AreEqual(1, ManagedConnectionFactory.SharedConnections.Count);

            // Action
            rmqConnection.ConnectionShutdown += Raise.EventWith(rmqConnection, new ShutdownEventArgs(ShutdownInitiator.Application, 0, "Connection dropped for unknow reason ;)"));

            //Assert
            Assert.AreEqual(0, ManagedConnectionFactory.SharedConnections.Count);
            retryPolicy.Received().WaitForNextRetry(Arg.Any<Action>());
        }
    }
}
// ReSharper restore InconsistentNaming