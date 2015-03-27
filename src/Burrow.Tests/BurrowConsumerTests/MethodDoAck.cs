using System;
using System.IO;
using System.Threading;
using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestFixture]
    public class MethodDoAck
    {
        [Test]
        public void Should_do_nothing_if_not_connected()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var msgHandler = Substitute.For<IMessageHandler>();
            var consumer = new BurrowConsumerForTest(model, msgHandler, Substitute.For<IRabbitWatcher>(), true, 3) { ConsumerTag = "ConsumerTag" };

            // Action
            consumer.DoAckForTest(new BasicDeliverEventArgs(), consumer);


            // Assert
            model.DidNotReceive().BasicAck(Arg.Any<ulong>(), Arg.Any<bool>());
            consumer.Dispose();
        }

        [Test]
        public void Should_do_nothing_if_already_disposed()
        {
            // Arrange
            var waitHandler = new ManualResetEvent(false);
            var model = Substitute.For<IModel>();
            var consumer = new BurrowConsumerForTest(model, Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), true, 3) { ConsumerTag = "ConsumerTag" };
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => waitHandler.WaitOne());
           

            // Action
            consumer.Dispose();
            consumer.DoAckForTest(new BasicDeliverEventArgs(), consumer);

            // Assert
            model.DidNotReceive().BasicAck(Arg.Any<ulong>(), Arg.Any<bool>());
            waitHandler.Set();
        }


        [Test]
        public void Should_catch_AlreadyClosedException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>()))
                 .Do(callInfo => { throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown")); });

            var msgHandler = Substitute.For<IMessageHandler>();
            var watcher = Substitute.For<IRabbitWatcher>();
            var consumer = new BurrowConsumerForTest(model, msgHandler, watcher, true, 3) { ConsumerTag = "ConsumerTag" };

            // Action
            consumer.DoAckForTest(new BasicDeliverEventArgs(), consumer);


            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
            consumer.Dispose();
        }

        [Test]
        public void Should_catch_IOException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>()))
                 .Do(callInfo => { throw new IOException(); });

            var msgHandler = Substitute.For<IMessageHandler>();
            var watcher = Substitute.For<IRabbitWatcher>();
            var consumer = new BurrowConsumerForTest(model, msgHandler, watcher, true, 3) { ConsumerTag = "ConsumerTag" };

            // Action
            consumer.DoAckForTest(new BasicDeliverEventArgs(), consumer);


            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
            consumer.Dispose();
        }

        [Test, ExpectedException(typeof(Exception))]
        public void Should_throw_other_Exceptions()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>()))
                 .Do(callInfo => { throw new Exception(); });

            var msgHandler = Substitute.For<IMessageHandler>();
            var watcher = Substitute.For<IRabbitWatcher>();
            var consumer = new BurrowConsumerForTest(model, msgHandler, watcher, true, 3) { ConsumerTag = "ConsumerTag" };

            // Action
            consumer.DoAckForTest(new BasicDeliverEventArgs(), consumer);


            // Assert
            watcher.DidNotReceive().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming