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
            var blockTheThread = new AutoResetEvent(false);
            var waitForFirstDequeue = new AutoResetEvent(false);
            var countdownEvent = new CountdownEvent(1);

            var channel = Substitute.For<IModel>();
            channel.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => countdownEvent.Signal());

            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => { waitForFirstDequeue.Set(); blockTheThread.WaitOne(); });

            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());
            consumer.Ready();


            // Action
            waitForFirstDequeue.WaitOne();
            consumer.MessageHandlerHandlingComplete(new BasicDeliverEventArgs("Burrow", 1, true, "", "", null, null));

            // Assert
            countdownEvent.Wait();
            consumer.Dispose();
            blockTheThread.Set();
        }
    }

}
// ReSharper restore InconsistentNaming