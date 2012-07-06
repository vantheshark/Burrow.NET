using System.Threading;
using Burrow.Extras;
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
            var consumer = new BurrowConsumerForTest(model, msgHandler, Substitute.For<IRabbitWatcher>(), true, 3);


            // Action
            msgHandler.HandlingComplete += Raise.Event<MessageHandlingEvent>(Substitute.For<BasicDeliverEventArgs>());
            consumer.WaitHandler.WaitOne();

            // Assert
            model.Received().BasicAck(Arg.Any<ulong>(), false);
            consumer.Dispose();
        }

        [TestMethod]
        public void Should_handle_all_exception()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new SubscriptionNotFoundException("Ack error"); });
            var msgHandler = Substitute.For<IMessageHandler>();
            var watcher = Substitute.For<IRabbitWatcher>();
            watcher.When(w => w.Error(Arg.Any<SubscriptionNotFoundException>())).Do(callInfo => waitHandler.Set());

            var consumer = new BurrowConsumer(model, msgHandler, watcher, true, 3);


            // Action
            msgHandler.HandlingComplete += Raise.Event<MessageHandlingEvent>(Substitute.For<BasicDeliverEventArgs>());
            waitHandler.WaitOne();

            // Assert
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming