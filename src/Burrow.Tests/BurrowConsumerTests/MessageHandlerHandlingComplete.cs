using System.Threading;
using Burrow.Extras;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestFixture]
    public class MessageHandlerHandlingComplete
    {
        [Test]
        public void Should_be_executed_when_the_message_handler_complete_message()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            var consumer = new BurrowConsumerForTest(model, msgHandler, Substitute.For<IRabbitWatcher>(), true, 3) {ConsumerTag = "ConsumerTag"};


            // Action
            msgHandler.HandlingComplete += Raise.Event<MessageHandlingEvent>(BurrowConsumerForTest.ADeliverEventArgs);
            Assert.IsTrue(consumer.WaitHandler.WaitOne(5000), "Test wait timeout");

            // Assert
            model.Received().BasicAck(Arg.Any<ulong>(), false);
            consumer.Dispose();
        }

        [Test]
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

            var consumer = new BurrowConsumer(model, msgHandler, watcher, true, 3) { ConsumerTag = "ConsumerTag" };
            consumer.ConsumerTag = "ConsumerTag";

            // Action
            msgHandler.HandlingComplete += Raise.Event<MessageHandlingEvent>(BurrowConsumerForTest.ADeliverEventArgs);
            Assert.IsTrue(waitHandler.WaitOne(5000), "Test wait timeout");

            // Assert
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming