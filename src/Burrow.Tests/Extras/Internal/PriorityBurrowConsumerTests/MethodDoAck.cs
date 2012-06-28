using System;
using System.IO;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestClass]
    public class MethodDoAck
    {
        [TestMethod]
        public void Should_do_nothing_if_already_disposed()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var consumer = new PriorityBurrowConsumer(model, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), false, 1);
            consumer.Init(Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>(), new CompositeSubscription(), 1, "sem");

            // Action
            consumer.Dispose();
            consumer.DoAck(new BasicDeliverEventArgs());


            // Assert
            model.DidNotReceive().BasicAck(Arg.Any<ulong>(), Arg.Any<bool>());
        }


        [TestMethod]
        public void Should_catch_AlreadyClosedException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown")); });
            var watcher = Substitute.For<IRabbitWatcher>();
            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(model) { ConsumerTag = "Burrow" });

            var consumer = new PriorityBurrowConsumer(model, Substitute.For<IMessageHandler>(), watcher, false, 1);
            consumer.Init(Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>(), sub, 1, "sem");

            // Action

            consumer.DoAck(new BasicDeliverEventArgs{ConsumerTag = "Burrow"});


            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
            consumer.Dispose();
        }

        [TestMethod]
        public void Should_catch_IOException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new IOException(); });
            var watcher = Substitute.For<IRabbitWatcher>();
            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(model) { ConsumerTag = "Burrow" });

            var consumer = new PriorityBurrowConsumer(model, Substitute.For<IMessageHandler>(), watcher, false, 1);
            consumer.Init(Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>(), sub, 1, "sem");

            // Action

            consumer.DoAck(new BasicDeliverEventArgs { ConsumerTag = "Burrow" });


            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
            consumer.Dispose();
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_other_Exceptions()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new Exception(); });
            var watcher = Substitute.For<IRabbitWatcher>();
            var sub = Substitute.For<CompositeSubscription>();
            sub.AddSubscription(new Subscription(model) { ConsumerTag = "Burrow" });

            var consumer = new PriorityBurrowConsumer(model, Substitute.For<IMessageHandler>(), watcher, false, 1);
            consumer.Init(Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>(), sub, 1, "sem");

            // Action

            consumer.DoAck(new BasicDeliverEventArgs { ConsumerTag = "Burrow" });


            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming