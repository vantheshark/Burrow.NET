using System;
using System.IO;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodDispose
    {
        [TestMethod]
        public void Should_dispose_everything()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Subscribe<Customer>("subscriptionName", x => { });

            // Action
            tunnel.Dispose();
            System.Threading.Thread.Sleep(100);

            // Assert
            // One for the dedicated publish channel and the other for the above subcribe channel
            newChannel.Received(2).Dispose();
            newChannel.Received(2).Abort();
            durableConnection.Received(1).Dispose();
        }

        [TestMethod]
        public void Should_not_throw_IOException()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.When(x => x.Abort()).Do(callInfo => { throw new IOException();});
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Subscribe<Customer>("subscriptionName", x => { });

            // Action
            tunnel.Dispose();
            System.Threading.Thread.Sleep(100);

            // Assert
            durableConnection.Received(1).Dispose();
        }

        [TestMethod]
        public void Should_not_throw_any_other_Exception()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.When(x => x.Abort()).Do(callInfo => { throw new Exception(); });
            IDurableConnection durableConnection;
            var tunnel = RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Subscribe<Customer>("subscriptionName", x => { });

            // Action
            tunnel.Dispose();
            System.Threading.Thread.Sleep(100);

            // Assert
            durableConnection.Received(1).Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming