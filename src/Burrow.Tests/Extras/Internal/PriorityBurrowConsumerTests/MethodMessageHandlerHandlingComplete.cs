using System;
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
    public class MethodMessageHandlerHandlingComplete
    {
        [TestMethod]
        public void Should_release_the_pool()
        {
            // Arrange
            var blockTheThread = new AutoResetEvent(false);
            var countdownEvent = new CountdownEvent(1);

            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => { countdownEvent.Signal(); blockTheThread.WaitOne(); });
            
            var consumer = new PriorityBurrowConsumer(Substitute.For<IModel>(), Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), false, 1);
            consumer.Init(queue, Substitute.For<CompositeSubscription>(), 1, Guid.NewGuid().ToString());
            consumer.Ready();
            

            // Action
            countdownEvent.Wait();
            countdownEvent.Reset();
            blockTheThread.Set();
            consumer.MessageHandlerHandlingComplete(null);
            countdownEvent.Wait();
            // Assert
            
            queue.Received(2).Dequeue();
            consumer.Dispose();
            blockTheThread.Dispose();
        }

        [TestMethod]
        public void Should_ack_if_auto_ack()
        {
            // Arrange
            var waitHandler = new ManualResetEvent(false);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => waitHandler.WaitOne());

            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => waitHandler.Set());

            var handler = Substitute.For<IMessageHandler>();
            var subscription = new CompositeSubscription();
            subscription.AddSubscription(new Subscription(model) {ConsumerTag = "ConsumerTag"});

            var consumer = new PriorityBurrowConsumer(model, handler, Substitute.For<IRabbitWatcher>(), true, 1);
            consumer.Init(queue, subscription, 1, Guid.NewGuid().ToString());
            consumer.Ready();

            // Action
            handler.HandlingComplete += Raise.Event<MessageHandlingEvent>(new BasicDeliverEventArgs{ConsumerTag = "ConsumerTag"});
            waitHandler.WaitOne();

            // Assert
            model.Received(1).BasicAck(Arg.Any<ulong>(), false);
            consumer.Dispose();
        }

        [TestMethod]
        public void Should_not_ack_if_not_auto_ack()
        {
            // Arrange
            var waitHandler = new ManualResetEvent(false);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => waitHandler.WaitOne());

            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);

            var handler = Substitute.For<IMessageHandler>();
            var subscription = new CompositeSubscription();
            subscription.AddSubscription(new Subscription(model) { ConsumerTag = "ConsumerTag" });

            var consumer = new PriorityBurrowConsumer(model, handler, Substitute.For<IRabbitWatcher>(), false, 1);
            consumer.Init(queue, subscription, 1, Guid.NewGuid().ToString());
            consumer.Ready();

            // Action
            handler.HandlingComplete += Raise.Event<MessageHandlingEvent>(new BasicDeliverEventArgs { ConsumerTag = "ConsumerTag" });
            
            // Assert
            model.DidNotReceive().BasicAck(Arg.Any<ulong>(), Arg.Any<bool>());
            waitHandler.Set();
            consumer.Dispose();
        }
    }

}
// ReSharper restore InconsistentNaming