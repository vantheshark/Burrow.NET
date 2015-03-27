using System.Threading;
using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestFixture]
    public class MethodDoAck
    {
        [Test]
        public void Should_do_nothing_if_already_disposed()
        {
            // Arrange
            var waitHandler = new ManualResetEvent(false);
            var model = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(model, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), false, 1);
            consumer.ConsumerTag = "ConsumerTag";
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => waitHandler.WaitOne());
            consumer.Init(queue, new CompositeSubscription(), 1, "sem");
            consumer.Ready();

            // Action
            consumer.Dispose();
            consumer.DoAck(new BasicDeliverEventArgs
                               {
                                   ConsumerTag = "ConsumerTag"
                               }, consumer);

            // Assert
            model.DidNotReceive().BasicAck(Arg.Any<ulong>(), Arg.Any<bool>());
            waitHandler.Set();
        }
    }
}
// ReSharper restore InconsistentNaming