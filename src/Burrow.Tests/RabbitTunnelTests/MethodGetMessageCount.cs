using System;
using System.IO;
using System.Threading;
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
            durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());
            durableConnection.CreateChannel().Returns(newChannel);
            var tunnel = new RabbitTunnel(routeFinder, durableConnection);

            // Action
            var count = tunnel.GetMessageCount<Customer>("subscriptionName");

            // Assert
            Assert.AreEqual((uint)100, count);
        }
    }
}
// ReSharper restore InconsistentNaming