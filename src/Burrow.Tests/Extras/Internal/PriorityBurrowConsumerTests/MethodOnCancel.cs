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
    public class MethodOnCancel
    {
        [Test]
        public void Should_close_queue_and_end_other_thread()
        {
            // Arrange
            var channel = Substitute.For<IModel>();

            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => Thread.Sleep(100));

            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            consumer.ConsumerTag = "ConsumerTag";

            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(channel) { ConsumerTag = "ConsumerTag" });
            consumer.Init(queue, sub, 1, Guid.NewGuid().ToString());
            consumer.Ready();


            // Action
            consumer.OnCancel();

            // Assert
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming