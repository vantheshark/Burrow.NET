using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestFixture]
    public class MethodOpenTunnel
    {
        [Test]
        public void Should_close_dedicatedPublishChannel()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            IDurableConnection durableConnection;
            var tunnel = (RabbitTunnelForTest)RabbitTunnelForTest.CreateTunnel(newChannel, out durableConnection);
            tunnel.Publish(new Customer());
            
            // Action
            durableConnection.Disconnected += Raise.Event<Action>();
            newChannel.BasicAcks += Raise.EventWith<BasicAckEventArgs>();
            newChannel.BasicNacks += Raise.EventWith<BasicNackEventArgs>();
            newChannel.BasicReturn += Raise.EventWith<BasicReturnEventArgs>(); 

            // Assert
            Assert.IsNull(tunnel.OnBrokerReceivedMessageIsCall);
            Assert.IsNull(tunnel.OnBrokerRejectedMessageIsCall);
            Assert.IsNull(tunnel.OnMessageIsUnroutedIsCall);
        }
    }
}
// ReSharper restore InconsistentNaming