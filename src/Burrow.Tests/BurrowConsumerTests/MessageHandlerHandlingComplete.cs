using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestClass]
    public class MessageHandlerHandlingComplete
    {
        [TestMethod]
        public void Should_be_executed_when_the_message_handler_complete_message()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            var consumer = new BurrowConsumerForTest(model, msgHandler,
                                                     Substitute.For<IRabbitWatcher>(), "consumerTag", true, 3);



            // Action
            msgHandler.HandlingComplete += Raise.Event<MessageHandlingEvent>(Substitute.For<BasicDeliverEventArgs>());
            consumer.WaitHandler.WaitOne();

            // Assert
            model.Received().BasicAck(Arg.Any<ulong>(), false);
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming