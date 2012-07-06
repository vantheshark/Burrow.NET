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
            handler.Received(1).HandleMessage(Arg.Any<BasicDeliverEventArgs>());
            consumer.Dispose();
        }

        [TestMethod]
        public void When_called_should_throw_BadMessageHandlerException_if_handler_error()
        {
            // Arrange
            var waitHandler = new ManualResetEvent(false);
            var watcher = Substitute.For<IRabbitWatcher>();
            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            msgHandler.When(x => x.HandleMessage(Arg.Any<BasicDeliverEventArgs>())).Do(callInfo =>{
                waitHandler.Set();
                throw new Exception("Bad excepton");
            });

            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => waitHandler.WaitOne());

            var consumer = new PriorityBurrowConsumer(channel, msgHandler, watcher, true, 1);

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());
            consumer.Ready();


            // Action
            try
            {
                consumer.HandleMessageDelivery(new BasicDeliverEventArgs
                {
                    BasicProperties = Substitute.For<IBasicProperties>(),
                    ConsumerTag = "Burrow"
                });
            }
            catch (BadMessageHandlerException)
            {
                waitHandler.Set();
            }
        }

        [TestMethod]
        public void When_called_should_dispose_the_thread_if_the_message_handler_throws_exception()
        {
            var waitHandler = new ManualResetEvent(false);
            
            var watcher = Substitute.For<IRabbitWatcher>();
            watcher.When(x => x.Error(Arg.Any<BadMessageHandlerException>())).Do(callInfo => waitHandler.Set());

            var channel = Substitute.For<IModel>();
            channel.IsOpen.Returns(true);
            
            var msgHandler = Substitute.For<IMessageHandler>();
            msgHandler.When(x => x.HandleMessage(Arg.Any<BasicDeliverEventArgs>())).Do(callInfo =>
            {
                throw new Exception("Bad excepton");
            });

            var queue = new InMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>(10, new PriorityComparer<GenericPriorityMessage<BasicDeliverEventArgs>>());

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "Burrow" });

            var consumer = new PriorityBurrowConsumer(channel, msgHandler, watcher, true, 1);
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());
            consumer.Ready();

            
            // Action
            queue.Enqueue(new GenericPriorityMessage<BasicDeliverEventArgs>(new BasicDeliverEventArgs(), 1));
            waitHandler.WaitOne();


            // Assert
            watcher.Received(1).Error(Arg.Any<BadMessageHandlerException>());
        }
    }
}
// ReSharper restore InconsistentNaming