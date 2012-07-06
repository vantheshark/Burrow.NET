using System.Threading;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestClass]
    public class MethodDispose
    {
        [TestMethod]
        public void Can_called_many_times()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => waitHandler.WaitOne());
            var consumer = new PriorityBurrowConsumer(Substitute.For<IModel>(), Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), false, 1);
            consumer.Init(queue, new CompositeSubscription(), 1, "sem");
            consumer.Ready();

            // Action
            consumer.Dispose();
            consumer.Dispose();
            waitHandler.Set();
        }
    }
}
// ReSharper restore InconsistentNaming