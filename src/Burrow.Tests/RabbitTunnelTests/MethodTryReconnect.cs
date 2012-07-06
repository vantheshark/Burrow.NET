using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodTryReconnect
    {
        [TestMethod]
        public void Should_reconnect_if_user_ack_wrong_delivery_id()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var id = Guid.NewGuid();
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.When(x => x.Connect()).Do(callInfo =>
            {
                durableConnection.Connected += Raise.Event<Action>();
                durableConnection.IsConnected.Returns(true);
            });

            durableConnection.CreateChannel().Returns(channel);
            var tunnel = new RabbitTunnelForTest(routeFinder, durableConnection);
            Action action = () => waitHandler.Set();
            tunnel.SubscribeActions[id] = action;
            tunnel.CreatedChannels.Add(channel);
            

            // Action
            tunnel.PublicTryReconnect(channel, id, new ShutdownEventArgs(ShutdownInitiator.Peer, 406, "PRECONDITION_FAILED - unknown delivery tag 10000"));
            waitHandler.WaitOne(5000);

            // Assert
            Assert.IsTrue(tunnel.CreatedChannels.Count == 0);
            
        }

        [TestMethod]
        public void Should_log_exception_if_cannot_reconnect()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var id = Guid.NewGuid();
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.When(x => x.Connect()).Do(callInfo =>
            {
                durableConnection.Connected += Raise.Event<Action>();
                durableConnection.IsConnected.Returns(true);
            });

            var watcher = Substitute.For<IRabbitWatcher>();

            durableConnection.CreateChannel().Returns(channel);
            var tunnel = new RabbitTunnelForTest(Substitute.For<IConsumerManager>(), watcher, routeFinder, durableConnection, Substitute.For<ISerializer>(), Substitute.For<ICorrelationIdGenerator>(), true);
            Action action = () => { waitHandler.Set(); throw new Exception("Cannot reconnect"); };
            tunnel.SubscribeActions[id] = action;
            tunnel.CreatedChannels.Add(channel);
            

            // Action
            tunnel.ExecuteSubscription(id);
            waitHandler.WaitOne(5000);

            // Assert
            watcher.Received(1).Error(Arg.Is<Exception>(e => e.Message == "Cannot reconnect"));
            
        }
    }
}
// ReSharper restore InconsistentNaming