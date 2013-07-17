using System;
using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.RabbitTunnelWithPriorityQueuesSupportTests
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
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);

            // Action
            var count = tunnel.GetMessageCount<Customer>("subscriptionName", 4);

            // Assert
            Assert.AreEqual((uint)500, count);
        }

        [TestMethod]
        public void Should_catch_all_exception()
        {
            // Arrange
            var newChannel = Substitute.For<IModel>();
            newChannel.IsOpen.Returns(true);
            newChannel.When(x => x.QueueDeclarePassive(Arg.Any<string>()))
                      .Do(callInfo => { throw new Exception("Some errors happen");});
            
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.IsConnected.Returns(true);
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);

            // Action
            var count = tunnel.GetMessageCount<Customer>("subscriptionName", 4);

            // Assert
            Assert.AreEqual((uint)0, count);
            newChannel.Received(1).QueueDeclarePassive(Arg.Any<string>());
        }
    }
}
// ReSharper restore InconsistentNaming