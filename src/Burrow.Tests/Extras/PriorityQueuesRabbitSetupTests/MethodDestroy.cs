using System;
using Burrow.Extras;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.PriorityQueuesRabbitSetupTests
{
    [TestClass]
    public class MethodDestroy
    {
        [TestMethod]
        public void Should_act_as_deleting_normal_queue_if_not_provide_PriorityQueueSetupData()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = PriorityQueuesRabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.Destroy<Customer>(new ExchangeSetupData(), new QueueSetupData());

            // Assert
            model.Received().QueueDelete("Queue.Customer");
        }

        [TestMethod]
        public void Should_delete_all_PRIORITY_queues_if_provide_PriorityQueueSetupData()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = PriorityQueuesRabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.Destroy<Customer>(new HeaderExchangeSetupData(), new PriorityQueueSetupData(3));

            // Assert
            model.Received().QueueDelete("Queue.Customer_Priority0");
            model.Received().QueueDelete("Queue.Customer_Priority1");
            model.Received().QueueDelete("Queue.Customer_Priority2");
            model.Received().QueueDelete("Queue.Customer_Priority3");
        }

        [TestMethod]
        public void Should_not_throw_exception_if_cannot_delete_PRIORITY_queues()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDelete(Arg.Any<string>())).Do(callInfo =>
            {
                throw new Exception("Test Exception");
            });
            var setup = PriorityQueuesRabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.Destroy<Customer>(new HeaderExchangeSetupData(), new PriorityQueueSetupData(3));
            model.Received().QueueDelete("Queue.Customer_Priority0");
            model.Received().QueueDelete("Queue.Customer_Priority1");
            model.Received().QueueDelete("Queue.Customer_Priority2");
            model.Received().QueueDelete("Queue.Customer_Priority3");
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_when_trying_to_delete_none_exist_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDelete(Arg.Any<string>())).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "NOT_FOUND - no queue "));
            });
            var setup = PriorityQueuesRabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.Destroy<Customer>(new HeaderExchangeSetupData(), new PriorityQueueSetupData(3));
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_and_log_error_when_trying_to_delete_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDelete(Arg.Any<string>())).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Other error"));
            });
            var setup = PriorityQueuesRabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.Destroy<Customer>(new HeaderExchangeSetupData(), new PriorityQueueSetupData(3));
        }
    }
}
// ReSharper restore InconsistentNaming