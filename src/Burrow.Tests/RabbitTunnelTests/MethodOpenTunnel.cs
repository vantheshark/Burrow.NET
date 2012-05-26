using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodOpenTunnel
    {
        [TestMethod]
        public void Should_close_dedicatedPublishChannel()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = (RabbitTunnelForTest)RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Publish(new Customer());
            
            // Action
            durableConnection.Disconnected += Raise.Event<Action>();
            newChannel.BasicAcks += Raise.Event<BasicAckEventHandler>(null, null);
            newChannel.BasicNacks += Raise.Event<BasicNackEventHandler>(null, null);
            newChannel.BasicReturn += Raise.Event<BasicReturnEventHandler>(null, null); 

            // Assert
            Assert.IsNull(tunnel.OnBrokerReceivedMessageIsCall);
            Assert.IsNull(tunnel.OnBrokerRejectedMessageIsCall);
            Assert.IsNull(tunnel.OnMessageIsUnroutedIsCall);
        }
    }
}
// ReSharper restore InconsistentNaming