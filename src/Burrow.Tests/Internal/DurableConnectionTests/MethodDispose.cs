using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestClass]
    public class MethodDispose : DurableConnectionTestHelper
    {
        [TestMethod]
        public void Should_NOT_close_any_connection()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost19", out rmqConnection);

            var channel = Substitute.For<IModel>();
            rmqConnection.CreateModel().Returns(channel);

            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            var count = ManagedConnectionFactory.SharedConnections.Count;
            durableConnection.Connect();
            durableConnection.CreateChannel();
            Assert.AreEqual(count + 1, ManagedConnectionFactory.SharedConnections.Count);


            // Action
            durableConnection.Dispose();

            //Assert
            rmqConnection.DidNotReceive().Close(Arg.Any<ushort>(), Arg.Any<string>());
            rmqConnection.DidNotReceive().Dispose();
            Assert.AreEqual(count + 1, ManagedConnectionFactory.SharedConnections.Count);
        }
    }
}
// ReSharper restore InconsistentNaming