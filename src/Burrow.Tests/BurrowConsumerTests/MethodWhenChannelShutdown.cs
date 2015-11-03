
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestFixture]
    public class MethodWhenChannelShutdown
    {
        [Test]
        public void Should_be_executed_when_the_channel_is_shutdown()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            var consumer = new BurrowConsumerForTest(model, msgHandler,
                                                     Substitute.For<IRabbitWatcher>(), true, 3) { ConsumerTag = "ConsumerTag" };

            // Action
            model.ModelShutdown += Raise.EventWith(model, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown"));
            

            // Assert
            Assert.IsTrue(consumer.WaitHandler.WaitOne(5000), "Test wait timeout");
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming