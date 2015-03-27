using System;
using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestFixture]
    public class MethodInit
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_subscription_is_null()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();

            // Action
            consumer.Init(queue, null, 1, "sem");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_PriorityQueue_is_null()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            

            // Action
            consumer.Init(null, new CompositeSubscription(), 1, "sem");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_semaphore_name_is_null()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();

            // Action
            consumer.Init(queue, new CompositeSubscription(), 1, null);
        }

        [Test]
        public void Should_delete_all_existing_msgs_that_have_same_priority()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            var queue = new InMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>(5, new PriorityComparer<GenericPriorityMessage<BasicDeliverEventArgs>>());
            queue.Enqueue(new GenericPriorityMessage<BasicDeliverEventArgs>(new BasicDeliverEventArgs(), 2));
            queue.Enqueue(new GenericPriorityMessage<BasicDeliverEventArgs>(new BasicDeliverEventArgs(), 2));
            queue.Enqueue(new GenericPriorityMessage<BasicDeliverEventArgs>(new BasicDeliverEventArgs(), 2));
            queue.Enqueue(new GenericPriorityMessage<BasicDeliverEventArgs>(new BasicDeliverEventArgs(), 2));
            

            // Action
            consumer.Init(queue, new CompositeSubscription(), 2, "sem");

            // Assert
            Assert.AreEqual(0, queue.Count);
        }
    }
}
// ReSharper restore InconsistentNaming