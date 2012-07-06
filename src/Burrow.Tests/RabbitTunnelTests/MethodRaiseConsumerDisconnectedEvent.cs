using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodRaiseConsumerDisconnectedEvent
    {
        [TestMethod]
        public void Should_raise_event()
        {
            // Arrange
            var isCalled = false;
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            routeFinder.When(x => x.FindExchangeName<string>()).Do(callInfo => { throw new Exception("Test message"); });
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.When(x => x.Connect()).Do(callInfo =>
            {
                durableConnection.Connected += Raise.Event<Action>();
                durableConnection.IsConnected.Returns(true);
            });

            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnelForTest(routeFinder, durableConnection);
            tunnel.ConsumerDisconnected += s => { isCalled = true; };

            // Action
            tunnel.PublicRaiseConsumerDisconnectedEvent(new Subscription(newChannel));
            Assert.IsTrue(isCalled);
        }
    }
}
// ReSharper restore InconsistentNaming