using System;
using System.IO;
using System.Threading;
using Burrow.Extras.Internal;
using Burrow.Tests.BurrowConsumerTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestClass]
    public class MethodReady
    {
        static MethodReady()
        {
            Global.ConsumerDisposeTimeoutInSeconds = 1;
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_subscription_is_null()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);

            // Action
            consumer.Ready();
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_PriorityQueue_is_null()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            consumer.Init(queue, new CompositeSubscription(), 1, "sem");
            consumer.PriorityQueue = null;

            // Action
            consumer.Ready();
        }

        [TestMethod]
        public void Should_start_a_thread_to_dequeue_on_priority_queue()
        {
            // Arrange
            var dequeueCount = new AutoResetEvent(false);
            var enqueueCount = new AutoResetEvent(false);
            var channel = Substitute.For<IModel>();
            
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.Dequeue().Returns(new GenericPriorityMessage<BasicDeliverEventArgs>(new BasicDeliverEventArgs(), 1));
            queue.When(x => x.Dequeue()).Do(callInfo => dequeueCount.Set());
            queue.When(x => x.Enqueue(Arg.Any<GenericPriorityMessage<BasicDeliverEventArgs>>()))
                 .Do(callInfo => enqueueCount.Set());

            var handler = Substitute.For<IMessageHandler>();
            handler.When(h => h.HandleMessage(Arg.Any<BasicDeliverEventArgs>()))
                   .Do(callInfo => handler.HandlingComplete += Raise.Event<MessageHandlingEvent>(BurrowConsumerForTest.ADeliverEventArgs));

            var consumer = new PriorityBurrowConsumer(channel, handler, Substitute.For<IRabbitWatcher>(), true, 1);
                               

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());

            // Action
            consumer.Ready();
            dequeueCount.WaitOne();
            consumer.PriorityQueue.Enqueue(new GenericPriorityMessage<BasicDeliverEventArgs>(new BasicDeliverEventArgs(), 1));
            enqueueCount.WaitOne();
            
            

            // Assert
            consumer.Dispose();
        }

        [TestMethod]
        public void Should_catch_EndOfStreamException()
        {
            // Arrange
            var count = new CountdownEvent(2);
            var channel = Substitute.For<IModel>();
            var watcher = Substitute.For<IRabbitWatcher>();
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => {
                count.Signal();
                throw new EndOfStreamException();                
            });
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), watcher, true, 2);

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());
            

            // Action
            consumer.Ready();
            count.Wait();

            // Assert
            consumer.Dispose();
        }

        [TestMethod]
        public void Should_catch_ThreadStateException()
        {
            // Arrange
            var count = new AutoResetEvent(false);
            var channel = Substitute.For<IModel>();
            var watcher = Substitute.For<IRabbitWatcher>();
            watcher.When(w => w.WarnFormat(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<uint>(), Arg.Any<string>(), Arg.Any<string>())).Do(c => count.Set());
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo =>
            {
                throw new ThreadStateException();
            });
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), watcher, true, 2);

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());


            // Action
            consumer.Ready();
            count.WaitOne(2000);

            // Assert
            consumer.Dispose();
        }

        [TestMethod]
        public void Should_catch_ThreadInterruptedException()
        {
            // Arrange
            var count = new AutoResetEvent(false);
            var channel = Substitute.For<IModel>();
            var watcher = Substitute.For<IRabbitWatcher>();
            watcher.When(w => w.WarnFormat(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<uint>(), Arg.Any<string>(), Arg.Any<string>())).Do(c => count.Set());
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo =>
            {
                throw new ThreadInterruptedException();
            });
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), watcher, true, 2);

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());


            // Action
            consumer.Ready();
            count.WaitOne(2000);

            // Assert
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming