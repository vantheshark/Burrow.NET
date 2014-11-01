using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    /// <summary>
    /// This method is executed automatically from the constructor
    /// </summary>
    [TestClass]
    public class MethodWaitAndHandleMessageDelivery
    {
        [TestMethod]
        public void Should_log_error_if_arrived_message_is_null()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var msgHandler = Substitute.For<IMessageHandler>();
            var watcher = Substitute.For<IRabbitWatcher>();
            var consumer = new BurrowConsumerForTest(model, msgHandler, watcher, true, 1) { ConsumerTag = "ConsumerTag" };
            

            // Action
            consumer.Queue.Enqueue(null);
            System.Threading.Thread.Sleep(500);

            // Assert
            watcher.Received(1).ErrorFormat("Message arrived but it's null for some reason, properly a serious BUG :D, contact author asap, release semaphore for other messages");
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming