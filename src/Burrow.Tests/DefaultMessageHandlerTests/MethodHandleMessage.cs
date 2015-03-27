using System;
using System.Threading;
using Burrow.Tests.Extras.RabbitSetupTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{
    [TestFixture]
    public class MethodHandleMessage
    {
        [Test]
        public void Should_fire_event_HandlingComplete()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            var handler = new DefaultMessageHandler<Customer>("SubscriptionName", Substitute.For<Action<Customer, MessageDeliverEventArgs>>(), errorHanlder, Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());
            handler.HandlingComplete += x => are.Set();
            var p = Substitute.For<IBasicProperties>();
            p.Type = Global.DefaultTypeNameSerializer.Serialize(typeof(Customer));

            // Action
            handler.HandleMessage(new BasicDeliverEventArgs("tag", 1, false, "e", "r", p, new byte[0]));

            // Assert
            Assert.IsTrue(are.WaitOne(1000));
            errorHanlder.DidNotReceive().HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
        }


        [Test]
        public void Should_handle_error_if_the_task_failed()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            Action<Customer, MessageDeliverEventArgs> taskFactory = (x, y) => { throw new Exception("Task executed failed"); };
            var handler = new DefaultMessageHandler<Customer>("SubscriptionName", taskFactory, errorHanlder, Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());
            handler.HandlingComplete += x => are.Set();

            // Action
            handler.HandleMessage(new BasicDeliverEventArgs("tag", 1, false, "e", "r", Substitute.For<IBasicProperties>(), new byte[0]));

            // Assert
            Assert.IsTrue(are.WaitOne(1000));
            errorHanlder.Received().HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>());
            
        }

        [Test]
        public void Should_not_throw_any_exception()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var errorHanlder = Substitute.For<IConsumerErrorHandler>();
            errorHanlder.When(x => x.HandleError(Arg.Any<BasicDeliverEventArgs>(), Arg.Any<Exception>())).Do(callInfo => { throw new Exception("Cannot handle error"); });
            Action<Customer, MessageDeliverEventArgs> taskFactory = (x, y) => { throw new Exception("Task executed failed"); };
            var watcher = Substitute.For<IRabbitWatcher>();
            var handler = new DefaultMessageHandler<Customer>("SubscriptionName", taskFactory, errorHanlder, Substitute.For<ISerializer>(), watcher);
            handler.HandlingComplete += x => are.Set();

            var p = Substitute.For<IBasicProperties>();
            p.Type = Global.DefaultTypeNameSerializer.Serialize(typeof (Customer));

            // Action
            handler.HandleMessage(new BasicDeliverEventArgs("tag", 1, false, "e", "r", p, new byte[0]));

            // Assert
            Assert.IsTrue(are.WaitOne(1000));
            watcher.Received().Error(Arg.Is<Exception>(x => x.Message == "Task executed failed"));
            watcher.Received().ErrorFormat(Arg.Is<string>(x => x.StartsWith("Failed to handle the exception: ")), Arg.Any<object>(), Arg.Any<object>());

        }
    }
}
// ReSharper restore InconsistentNaming