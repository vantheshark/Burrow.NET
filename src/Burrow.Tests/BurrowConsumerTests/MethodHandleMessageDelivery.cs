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
            msgHandler.When(x => x.HandleMessage(Arg.Any<IBasicConsumer>(), Arg.Any<BasicDeliverEventArgs>()))
                      .Do(callInfo => waitHandler.Set());
            var consumer = new BurrowConsumerForTest(model, msgHandler,
                                                     Substitute.For<IRabbitWatcher>(), "consumerTag", true, 3);

            // Action
            consumer.Queue.Enqueue(new BasicDeliverEventArgs
            {
                BasicProperties = Substitute.For<IBasicProperties>()
            });
            waitHandler.WaitOne();


            // Assert
            msgHandler.Received().BeforeHandlingMessage(consumer, Arg.Any<BasicDeliverEventArgs>());
            msgHandler.DidNotReceive().HandleError(Arg.Any<IBasicConsumer>(), Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
            consumer.Dispose();
        }

        [TestMethod]
        public void When_called_should_catch_all_exception()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            msgHandler.When(x => x.HandleMessage(Arg.Any<IBasicConsumer>(), Arg.Any<BasicDeliverEventArgs>()))
                      .Do(callInfo => { throw new Exception(); });
            var consumer = new BurrowConsumerForTest(model, msgHandler,
                                                     Substitute.For<IRabbitWatcher>(), "consumerTag", true, 3);

            // Action
            consumer.Queue.Enqueue(new BasicDeliverEventArgs
            {
                BasicProperties = Substitute.For<IBasicProperties>()
            });
            consumer.WaitHandler.WaitOne();

            // Assert
            msgHandler.Received().BeforeHandlingMessage(consumer, Arg.Any<BasicDeliverEventArgs>());
            msgHandler.Received().HandleError(Arg.Any<IBasicConsumer>(), Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
            consumer.Dispose();
        }


    }
}
// ReSharper restore InconsistentNaming