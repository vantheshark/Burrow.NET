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
    public class MethodChannelShutdown : DurableConnectionTestHelper
    {
        [TestMethod]
        public void Should_be_call_if_channel_is_shutdown()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory("/", out rmqConnection);
            var channel = Substitute.For<IModel>();
            rmqConnection.CreateModel().Returns(channel);

            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            durableConnection.Connect();
            durableConnection.CreateChannel();

            // Action
            channel.ModelShutdown += Raise.Event<ModelShutdownEventHandler>(channel, new ShutdownEventArgs(ShutdownInitiator.Application, 0, "Channel shutdown"));
            
            //Assert
            watcher.Received().WarnFormat("Channel shutdown: {0}", "Channel shutdown");
        }
    }
}
// ReSharper restore InconsistentNaming