using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestClass]
    public class MethodHandleMessageDelivery
    {
        [TestMethod]
        public void When_called_should_execute_methods_on_message_handler()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            //To decrease the messagages in progress so it doesn't have to wait when dispose at the end
            msgHandler.When(x => x.HandleMessage(Arg.Any<BasicDeliverEventArgs>()))
                      .Do(callInfo => msgHandler.HandlingComplete += Raise.Event<MessageHandlingEvent>(callInfo.Arg<BasicDeliverEventArgs>()));

            var watcher = Substitute.For<IRabbitWatcher>();
            watcher.IsDebugEnable.Returns(true);
            msgHandler.When(x => x.HandleMessage(Arg.Any<BasicDeliverEventArgs>()))
                      .Do(callInfo => waitHandler.Set());
            var consumer = new BurrowConsumer(model, msgHandler, watcher, true, 3); 

            // Action
            consumer.Queue.Enqueue(new BasicDeliverEventArgs
            {
                BasicProperties = Substitute.For<IBasicProperties>()
            });
            waitHandler.WaitOne();


            // Assert
            msgHandler.DidNotReceive().HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
            consumer.Dispose();
        }

        [TestMethod]
        public void When_called_should_dispose_if_the_message_handler_throws_exception()
        {
            var waitHandler = new ManualResetEvent(false);
            var watcher = Substitute.For<IRabbitWatcher>();
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            msgHandler.When(x => x.HandleMessage(Arg.Any<BasicDeliverEventArgs>()))
                .Do(callInfo =>
                    {
                        throw new Exception("Bad excepton");
                    }
                );

            watcher.When(x => x.Error(Arg.Any<Exception>())).Do(callInfo => waitHandler.Set());
            var consumer = new BurrowConsumer(model, msgHandler, watcher, true, 3);

            // Action
            consumer.Queue.Enqueue(new BasicDeliverEventArgs
            {
                BasicProperties = Substitute.For<IBasicProperties>()
            });
            waitHandler.WaitOne();

            // Assert
            watcher.Received(1).Error(Arg.Any<Exception>());
            Assert.IsTrue(consumer.IsDisposed);
        }
    }
}
// ReSharper restore InconsistentNaming