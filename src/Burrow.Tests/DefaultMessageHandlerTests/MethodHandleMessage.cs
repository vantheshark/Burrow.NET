using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{
    [TestClass]
    public class MethodHandleMessage
    {
        [TestMethod]
        public void Should_fire_event_HandlingComplete()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            Func<BasicDeliverEventArgs, Task> taskFactory = x => Task.Factory.StartNew(() => { });
            var handler = new DefaultMessageHandler(errorHanlder, taskFactory, Substitute.For<IRabbitWatcher>());
            handler.HandlingComplete += x => are.Set();


            // Action
            handler.HandleMessage(Substitute.For<IBasicConsumer>(), new BasicDeliverEventArgs("tag", 1, false, "e", "r", Substitute.For<IBasicProperties>(), new byte[0]));

            // Assert
            are.WaitOne();
            errorHanlder.DidNotReceive().HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
        }


        [TestMethod]
        public void Should_handle_error_if_the_task_failed()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            Func<BasicDeliverEventArgs, Task> taskFactory = x => Task.Factory.StartNew(() => { throw new Exception("Task executed failed");});
            var handler = new DefaultMessageHandler(errorHanlder, taskFactory, Substitute.For<IRabbitWatcher>());
            handler.HandlingComplete += x => are.Set();

            // Action
            handler.HandleMessage(Substitute.For<IBasicConsumer>(), new BasicDeliverEventArgs("tag", 1, false, "e", "r", Substitute.For<IBasicProperties>(), new byte[0]));

            // Assert
            are.WaitOne();
            errorHanlder.Received().HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
            
        }

        [TestMethod]
        public void Should_not_throw_any_exception()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            errorHanlder.When(x => x.HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>())).Do(callInfo => { throw new Exception("Cannot handle error"); });
            Func<BasicDeliverEventArgs, Task> taskFactory = x => Task.Factory.StartNew(() => { throw new Exception("Task executed failed"); });
            var watcher = Substitute.For<IRabbitWatcher>();
            var handler = new DefaultMessageHandler(errorHanlder, taskFactory, watcher);
            handler.HandlingComplete += x => are.Set();

            // Action
            handler.HandleMessage(Substitute.For<IBasicConsumer>(), new BasicDeliverEventArgs("tag", 1, false, "e", "r", Substitute.For<IBasicProperties>(), new byte[0]));

            // Assert
            are.WaitOne();
            watcher.Received().Error(Arg.Is<Exception>(x => x.Message == "Cannot handle error"));

        }
    }
}
// ReSharper restore InconsistentNaming