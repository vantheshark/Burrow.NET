using System;
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
    public class MethodHandleBasicDeliver
    {
        [Test]
        public void Should_put_msg_to_queue()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var handler = Substitute.For<IMessageHandler>();
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => Thread.Sleep(100));

            var consumer = new PriorityBurrowConsumer(channel, handler, Substitute.For<IRabbitWatcher>(), true, 10);
            consumer.ConsumerTag = "ConsumerTag";
            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "ConsumerTag" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());
            consumer.Ready();


            // Action
            consumer.HandleBasicDeliver("",0, false,"", "", null, null);

            // Assert
            queue.Received(1).Enqueue(Arg.Any<GenericPriorityMessage<BasicDeliverEventArgs>>());
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming