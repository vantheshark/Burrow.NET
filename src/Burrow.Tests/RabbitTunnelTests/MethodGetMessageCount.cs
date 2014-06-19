using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RabbitTunnelTests
{
    [TestClass]
    public class MethodGetMessageCount
    {
        [TestMethod]
        public void Should_return_the_messagecount()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.QueueDeclarePassive(Arg.Any<string>()).Returns(new QueueDeclareOk("", 100, 0));
            newChannel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.IsConnected.Returns(true);
            //durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnel(routeFinder, durableConnection);

            // Action
            var count = tunnel.GetMessageCount(new SubscriptionOption<Customer>
            {
                SubscriptionName = "subscriptionName"
            });

            // Assert
            Assert.AreEqual((uint)100, count);
        }

        [TestMethod]
        public void Should_return_0_if_QueueDeclarePassive_return_null()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            QueueDeclareOk declareResult = null;
            newChannel.QueueDeclarePassive(Arg.Any<string>()).Returns(declareResult);
            newChannel.IsOpen.Returns(true);
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.IsConnected.Returns(true);
            //durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnel(routeFinder, durableConnection);

            // Action
            var count = tunnel.GetMessageCount(new SubscriptionOption<Customer>
            {
                SubscriptionName = "subscriptionName"
            });

            // Assert
            Assert.AreEqual((uint)0, count);
        }

        [TestMethod]
        public void Should_catch_all_exception_and_return_0()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            newChannel.When(x => x.QueueDeclarePassive(Arg.Any<string>()))
                      .Do(callInfo => { throw new Exception("Some errors happen");});
            
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.IsConnected.Returns(true);
            //durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnel(routeFinder, durableConnection);

            // Action
            var count = tunnel.GetMessageCount(new SubscriptionOption<Customer>
            {
                SubscriptionName = "subscriptionName"
            });

            // Assert
            Assert.AreEqual((uint)0, count);
            newChannel.Received(1).QueueDeclarePassive(Arg.Any<string>());
        }
    }
}
// ReSharper restore InconsistentNaming