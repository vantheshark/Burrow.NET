using Burrow.Internal;
using Burrow.Tests.Internal.DurableConnectionTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.TunnelFactoryTests
{
    [TestFixture]
    public class MethodCloseAllConnections
    {
        [Test]
        public void Should_close_and_dispose_all_SharedConnections()
        {
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = DurableConnectionTestHelper.CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost22", out rmqConnection);
            var channel = Substitute.For<IModel>();
            rmqConnection.CreateModel().Returns(channel);

            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            durableConnection.CreateChannel();
            Assert.AreEqual(1, ManagedConnectionFactory.SharedConnections.Count);


            // Action
            RabbitTunnel.Factory.CloseAllConnections();

            //Assert
            rmqConnection.Received().Close(Arg.Any<ushort>(), Arg.Any<string>());
            rmqConnection.Received().Dispose();
            Assert.AreEqual(0, ManagedConnectionFactory.SharedConnections.Count);
        }
    }
}
// ReSharper restore InconsistentNaming