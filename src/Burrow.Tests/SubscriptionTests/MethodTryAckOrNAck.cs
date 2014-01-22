using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestClass]
    public class MethodTryAckOrNAck
    {
        [TestMethod]
        public void Should_do_nothing_if_channel_is_null()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            Subscription.TryAckOrNAck(x => x.BasicAck(0, false), null, watcher);

            // Assert
            watcher.Received(1).InfoFormat("Trying ack/nack msg but the Channel is null, will not do anything");
        }

        [TestMethod]
        public void Should_do_nothing_if_channel_is_not_open()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            Subscription.TryAckOrNAck(x => x.BasicAck(0, false), model, watcher);

            // Assert
            model.DidNotReceive().BasicAck(Arg.Any<ulong>(), Arg.Any<bool>());
        }

        [TestMethod]
        public void Should_catch_AlreadyClosedException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown")); });
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            Subscription.TryAckOrNAck(x => { throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Application, 0, ""));}, model, watcher);
            
            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [TestMethod]
        public void Should_catch_IOException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown")); });
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            Subscription.TryAckOrNAck(x => { throw new IOException(); }, model, watcher);

            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_other_Exceptions()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown")); });
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            Subscription.TryAckOrNAck(x => { throw new Exception("Other exceptions"); }, model, watcher);

            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}
// ReSharper restore InconsistentNaming