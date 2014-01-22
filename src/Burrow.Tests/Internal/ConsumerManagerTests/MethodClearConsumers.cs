using System;
using System.Threading;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsumerManagerTests
{
    [TestClass]
    public class MethodClearConsumers
    {
        [TestMethod]
        public void Should_catch_all_exception_when_dispose_consumers()
        {
            // Arrange
            var autoResetEvent = new AutoResetEvent(false);
            var model = Substitute.For<IModel>();
            var watcher = Substitute.For<IRabbitWatcher>();
            watcher.When(w => w.InfoFormat(Arg.Any<string>(), Arg.Any<object[]>()))
                   .Do(callinfo => { throw new Exception(); });

            var handlerFactory = Substitute.For<IMessageHandlerFactory>();
            var handler = Substitute.For<IMessageHandler>();
            handlerFactory.Create(Arg.Any<string>(), Arg.Any<Action<int, MessageDeliverEventArgs>>()).Returns(handler);
            handler.When(h => h.HandleMessage(Arg.Any<BasicDeliverEventArgs>()))
                   .Do(callInfo => autoResetEvent.Set());

            var consumerManager = new ConsumerManager(watcher, handlerFactory, Substitute.For<ISerializer>());
            var consumer = consumerManager.CreateConsumer<int>(model, "", x => { }, null);

            //To make it wait when dispose the BurrowConsumer
            ((QueueingBasicConsumer)consumer).Queue.Enqueue(new BasicDeliverEventArgs());
            

            // Action
            autoResetEvent.WaitOne();
            consumerManager.ClearConsumers();

            // Assert
            watcher.Received(1).Error(Arg.Any<Exception>());
        }
    }
}
// ReSharper restore InconsistentNaming