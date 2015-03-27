using System;
using System.IO;

using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestFixture]
    public class MethodTryAckOrNAck
    {
        [Test]
        public void Should_do_nothing_if_channel_is_null()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            Subscription.TryAckOrNack("", true, null, 0, true, false, watcher);

            // Assert
            watcher.Received(1).WarnFormat("Trying ack/nack msg but the Channel is null, will not do anything");
        }

        [Test]
        public void Should_do_nothing_if_channel_is_not_open()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            Subscription.TryAckOrNack("", true, model, 0, true, false, watcher);

            // Assert
            model.DidNotReceive().BasicAck(Arg.Any<ulong>(), Arg.Any<bool>());
        }

        [Test]
        public void Should_catch_AlreadyClosedException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new AlreadyClosedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown")); });
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            Subscription.TryAckOrNack("", true, model, 100, false, false, watcher);
            
            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void Should_catch_IOException()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicAck(Arg.Any<ulong>(), Arg.Any<bool>())).Do(callInfo => { throw new IOException(); });
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            //Subscription.TryAckOrNAck(x => { throw new IOException(); }, model, watcher);
            Subscription.TryAckOrNack("", true, model, 100, false, false, watcher);

            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test, ExpectedException(typeof(Exception))]
        public void Should_throw_other_Exceptions()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            model.When(x => x.BasicNack(Arg.Any<ulong>(), Arg.Any<bool>(), Arg.Any<bool>())).Do(callInfo => { throw new Exception(); });
            var watcher = Substitute.For<IRabbitWatcher>();

            // Action
            //Subscription.TryAckOrNAck(x => { throw new Exception("Other exceptions"); }, model, watcher);
            Subscription.TryAckOrNack("", false, model, 100, false, false, watcher);

            // Assert
            watcher.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}
// ReSharper restore InconsistentNaming