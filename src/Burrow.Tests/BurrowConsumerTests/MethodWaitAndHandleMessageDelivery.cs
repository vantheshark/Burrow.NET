using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestClass]
    public class MethodWaitAndHandleMessageDelivery
    {
        [TestMethod]
        public void Should_log_error_if_arrived_is_not_BasicDeliverEventArgs()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var msgHandler = Substitute.For<IMessageHandler>();
            var watcher = Substitute.For<IRabbitWatcher>();
            var consumer = new BurrowConsumerForTest(model, msgHandler, watcher, true, 1);
            consumer.Queue.Enqueue(1);

            // Action
            consumer.WaitAndHandleMessageDelivery();


            // Assert
            watcher.Received(1).ErrorFormat("Message arrived but it's not a BasicDeliverEventArgs for some reason, properly a serious BUG :D, contact author asap, release semaphore for other messages");

        }
    }
}
// ReSharper restore InconsistentNaming