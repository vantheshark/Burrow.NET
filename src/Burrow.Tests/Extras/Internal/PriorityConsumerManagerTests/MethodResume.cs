using System.Threading;
using Burrow.Extras.Internal;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityConsumerManagerTests
{
    [TestClass]
    public class MethodResume
    {
        readonly IRabbitWatcher watcher = NSubstitute.Substitute.For<IRabbitWatcher>();
        readonly IModel channel = NSubstitute.Substitute.For<IModel>();
        private readonly IMessageHandler handler = NSubstitute.Substitute.For<IMessageHandler>();

        [TestMethod]
        public void Should_return_PriorityBurrowConsumer_object_when_called()
        {
            // Arrange
            var consumerManager = new PriorityBurrowConsumerForResumingTest(new SharedEventBroker(NSubstitute.Substitute.For<IRabbitWatcher>()), 2, channel, handler, watcher, "consumerTag", false, 10);
            consumerManager.Interupt(1);
            var timer = new Timer(x => consumerManager.Resume(1), null, 1000, Timeout.Infinite);

            // Action
            consumerManager.Pool.WaitOne();

            // Assert
            timer.Dispose();
        }

        private class PriorityBurrowConsumerForResumingTest : PriorityBurrowConsumer
        {
            public PriorityBurrowConsumerForResumingTest(SharedEventBroker eventBroker, int priority, IModel channel, IMessageHandler messageHandler, IRabbitWatcher watcher, string consumerTag, bool autoAck, int batchSize) 
                : base(eventBroker, priority, channel, messageHandler, watcher, consumerTag, autoAck, batchSize)
            {
            }

            public InteruptableSemaphore Pool
            {
                get { return _pool; }
            }
        }
    }
}
// ReSharper restore InconsistentNaming