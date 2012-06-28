using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestClass]
    public class MethodWhenChannelShutdown
    {
        [TestMethod]
        public void Should_be_executed_when_the_channel_is_shutdown()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            var consumer = new BurrowConsumerForTest(model, msgHandler,
                                                     Substitute.For<IRabbitWatcher>(), true, 3);

            // Action
            model.ModelShutdown += Raise.Event<ModelShutdownEventHandler>(model, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown"));
            

            // Assert
            consumer.WaitHandler.WaitOne();
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming