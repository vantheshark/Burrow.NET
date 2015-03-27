using System;
using System.Threading;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{

    [TestFixture]
    public class MethodCleanUp
    {
        [Test]
        public void Should_catch_all_exception()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            var handler = new DefaultMessageHandlerForTest<Customer>(
                "SubscriptionName", 
                Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), 
                errorHanlder, 
                Substitute.For<ISerializer>(),
                watcher);

            // Action
            handler.CleanUp(new BasicDeliverEventArgs(), true);

            // Assert
            watcher.Received(1).Error(Arg.Is<Exception>(e => e.Message == "HandlingCompleteException"));
            watcher.Received(1).Error(Arg.Is<Exception>(e => e.Message == "AfterHandlingMessageException"));

        }


        [Test]
        public void Should_fire_event_MessageWasNotHandled_if_msg_was_not_handled()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var watcher = Substitute.For<IRabbitWatcher>();
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            var handler = new DefaultMessageHandlerForTest<Customer>(
                "SubscriptionName",
                Substitute.For<Action<Customer, MessageDeliverEventArgs>>(),
                errorHanlder,
                Substitute.For<ISerializer>(),
                watcher);
            handler.MessageWasNotHandled += x => waitHandler.Set();

            // Action
            handler.CleanUp(new BasicDeliverEventArgs(), false);
            Assert.IsTrue(waitHandler.WaitOne(1000));

            // Assert
            watcher.DidNotReceive().ErrorFormat(Arg.Is<string>(e => e == "There is an error when trying to fire MessageWasNotDelivered event"), Arg.Any<object[]>());
        }

        [Test]
        public void Should_catch_all_exception_when_firing_event_MessageWasNotHandled()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var watcher = Substitute.For<IRabbitWatcher>();
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            var handler = new DefaultMessageHandlerForTest<Customer>(
                "SubscriptionName",
                Substitute.For<Action<Customer, MessageDeliverEventArgs>>(),
                errorHanlder,
                Substitute.For<ISerializer>(),
                watcher);
            handler.MessageWasNotHandled += x => { waitHandler.Set(); throw new Exception("Error firing event MessageWasNotHandled"); };

            // Action
            handler.CleanUp(new BasicDeliverEventArgs(), false);
            Assert.IsTrue(waitHandler.WaitOne(1000));

            // Assert
            watcher.Received(1).ErrorFormat(Arg.Is<string>(e => e == "There is an error when trying to fire MessageWasNotDelivered event"), Arg.Any<object[]>());
            watcher.Received(1).Error(Arg.Is<Exception>(e => e.Message == "Error firing event MessageWasNotHandled"));
        }
    }
}
// ReSharper restore InconsistentNaming