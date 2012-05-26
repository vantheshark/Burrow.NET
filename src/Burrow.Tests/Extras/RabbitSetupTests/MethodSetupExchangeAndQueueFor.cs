using System;
using System.Collections;
using Burrow.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.RabbitSetupTests
{
    [TestClass]
    public class MethodSetupExchangeAndQueueFor
    {
        [TestMethod]
        public void Should_create_exchange_queues_and_bind_them()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.SetupExchangeAndQueueFor<Customer>(new ExchangeSetupData(), new QueueSetupData
            {
                AutoExpire = 10000,
                MessageTimeToLive = 10000000
            });

            // Assert
            model.Received().ExchangeDeclare("Exchange.Customer", "direct", true, false, null);
            model.Received().QueueDeclare("Queue.Customer", true, false, false, Arg.Any<IDictionary>());
            model.Received().QueueBind("Queue.Customer", "Exchange.Customer", "Customer");
        }

        [TestMethod]
        public void Should_not_throw_exception_if_cannot_declare_exchange()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.ExchangeDeclare("Exchange.Customer", "direct", true, false, null)).Do(callInfo =>
            {
                throw new Exception("Test Exception");
                                                                                                        
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.SetupExchangeAndQueueFor<Customer>(new ExchangeSetupData(), new QueueSetupData());
            
            // Assert
            model.Received().QueueDeclare("Queue.Customer", true, false, false, Arg.Any<IDictionary>());
            model.Received().QueueBind("Queue.Customer", "Exchange.Customer", "Customer");
        }

        [TestMethod]
        public void Should_not_throw_exception_if_cannot_declare_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDeclare("Queue.Customer", true, false, false, Arg.Any<IDictionary>())).Do(callInfo =>
            {
                throw new Exception("Test Exception");

            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.SetupExchangeAndQueueFor<Customer>(new ExchangeSetupData(), new QueueSetupData());

            // Assert
            model.Received().QueueBind("Queue.Customer", "Exchange.Customer", "Customer");
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_when_trying_to_create_an_exist_queue_but_configuration_not_match()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDeclare("Queue.Customer", true, false, false, Arg.Any<IDictionary>())).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "PRECONDITION_FAILED - "));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.SetupExchangeAndQueueFor<Customer>(new ExchangeSetupData(), new QueueSetupData());
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_and_log_error_when_trying_to_create_a_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDeclare("Queue.Customer", true, false, false, Arg.Any<IDictionary>())).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Other error"));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.SetupExchangeAndQueueFor<Customer>(new ExchangeSetupData(), new QueueSetupData());
        }

        [TestMethod]
        public void Should_not_throw_exception_if_cannot_bind_queues()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueBind("Queue.Customer", "Exchange.Customer", "Customer")).Do(callInfo =>
            {
                throw new Exception("Test Exception");

            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.SetupExchangeAndQueueFor<Customer>(new ExchangeSetupData(), new QueueSetupData());
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_when_trying_to_create_an_exist_exchange_but_configuration_not_match()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.ExchangeDeclare("Exchange.Customer", Arg.Any<string>(), true, false, null)).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "PRECONDITION_FAILED - "));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.SetupExchangeAndQueueFor<Customer>(new ExchangeSetupData(), new QueueSetupData());
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_and_log_error_when_trying_to_create_an_exchange()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.ExchangeDeclare("Exchange.Customer", Arg.Any<string>(), true, false, null)).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Other error"));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.SetupExchangeAndQueueFor<Customer>(new ExchangeSetupData(), new QueueSetupData());
        }
    }
}
// ReSharper restore InconsistentNaming