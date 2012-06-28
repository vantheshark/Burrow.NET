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
    public class MethodHandleMessageDelivery
    {
        [TestMethod]
        public void When_called_should_execute_methods_on_message_handler()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var handler = Substitute.For<IMessageHandler>();
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => Thread.Sleep(100));

            var consumer = new PriorityBurrowConsumer(channel, handler, Substitute.For<IRabbitWatcher>(), true, 1);

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());
            consumer.Ready();


            // Action
            consumer.HandleMessageDelivery(new BasicDeliverEventArgs
            {
                BasicProperties = Substitute.For<IBasicProperties>()
            });

            // Assert
            handler.Received(1).HandleMessage(consumer, Arg.Any<BasicDeliverEventArgs>());
            consumer.Dispose();
        }

        [TestMethod]
        public void When_called_should_catch_all_exception()
        {
            // Arrange
            var blockTheThread = new AutoResetEvent(false);
            var waitForFirstDequeue = new AutoResetEvent(false);

            var channel = Substitute.For<IModel>();
            var msgHandler = Substitute.For<IMessageHandler>();
            msgHandler.When(x => x.HandleMessage(Arg.Any<IBasicConsumer>(), Arg.Any<BasicDeliverEventArgs>()))
                      .Do(callInfo => { throw new Exception(); });

            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => { waitForFirstDequeue.Set(); blockTheThread.WaitOne(); });

            var consumer = new PriorityBurrowConsumer(channel, msgHandler, Substitute.For<IRabbitWatcher>(), true, 1);

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());
            consumer.Ready();


            // Action
            waitForFirstDequeue.WaitOne();
            consumer.HandleMessageDelivery(new BasicDeliverEventArgs
            {
                BasicProperties = Substitute.For<IBasicProperties>(),
                ConsumerTag = "Burrow"
            });

            // Assert
            msgHandler.Received().HandleError(Arg.Any<IBasicConsumer>(), Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
            consumer.Dispose();
            blockTheThread.Set();
        }
    }
}
// ReSharper restore InconsistentNaming