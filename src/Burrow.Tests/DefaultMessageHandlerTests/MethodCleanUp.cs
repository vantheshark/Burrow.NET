using System;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{

    [TestClass]
    public class MethodCleanUp
    {
        [TestMethod]
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
            handler.CleanUp(new BasicDeliverEventArgs());

            // Assert
            watcher.Received(1).Error(Arg.Is<Exception>(e => e.Message == "HandlingCompleteException"));
            watcher.Received(1).Error(Arg.Is<Exception>(e => e.Message == "AfterHandlingMessageException"));

        }
    }
}
// ReSharper restore InconsistentNaming