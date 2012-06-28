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
    public class MethodInit
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_subscription_is_null()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();

            // Action
            consumer.Init(queue, null, 1, "sem");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_PriorityQueue_is_null()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            

            // Action
            consumer.Init(null, new CompositeSubscription(), 1, "sem");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_semaphore_name_is_null()
        {
            // Arrange
            var channel = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(channel, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 1);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();

            // Action
            consumer.Init(queue, new CompositeSubscription(), 1, null);
        }
    }
}
// ReSharper restore InconsistentNaming