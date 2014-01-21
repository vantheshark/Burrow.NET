using System;
using System.Collections;
using System.Collections.Generic;
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
        private IRouteFinder _routeFinder = Substitute.For<IRouteFinder>();
        private RouteSetupData _routeSetupData;

        [TestInitialize]
        public void Setup()
        {
            _routeFinder.FindExchangeName<Customer>().Returns("Exchange.Customer");
            _routeFinder.FindQueueName<Customer>(null).ReturnsForAnyArgs("Queue.Customer");
            _routeFinder.FindRoutingKey<Customer>().Returns("Customer");

            _routeSetupData = new RouteSetupData
            {
                RouteFinder = _routeFinder,
                ExchangeSetupData = new ExchangeSetupData(),
                QueueSetupData = new QueueSetupData
                {
                    AutoExpire = 10000,
                    MessageTimeToLive = 10000000
                }
            };
        }

        [TestMethod]
        public void Should_create_exchange_queues_and_bind_them()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.CreateRoute<Customer>(_routeSetupData);

            // Assert
            model.Received().ExchangeDeclare("Exchange.Customer", "direct", true, false, _routeSetupData.ExchangeSetupData.Arguments);
            model.Received().QueueDeclare("Queue.Customer", true, false, false, _routeSetupData.QueueSetupData.Arguments);
            model.Received().QueueBind("Queue.Customer", "Exchange.Customer", "Customer", _routeSetupData.OptionalBindingData);
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
            setup.CreateRoute<Customer>(_routeSetupData);
            
            // Assert
            model.Received().QueueDeclare("Queue.Customer", true, false, false, _routeSetupData.QueueSetupData.Arguments);
            model.Received().QueueBind("Queue.Customer", "Exchange.Customer", "Customer", _routeSetupData.OptionalBindingData);
        }

        [TestMethod]
        public void Should_not_throw_exception_if_cannot_declare_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDeclare("Queue.Customer", true, false, false, Arg.Any<IDictionary<string, object>>())).Do(callInfo =>
            {
                throw new Exception("Test Exception");

            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.CreateRoute<Customer>(_routeSetupData);

            // Assert
            model.Received().QueueBind("Queue.Customer", "Exchange.Customer", "Customer", _routeSetupData.OptionalBindingData);
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_when_trying_to_create_an_exist_queue_but_configuration_not_match()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDeclare("Queue.Customer", true, false, false, Arg.Any<IDictionary<string, object>>())).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "PRECONDITION_FAILED - "));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.CreateRoute<Customer>(_routeSetupData);
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_and_log_error_when_trying_to_create_a_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDeclare("Queue.Customer", true, false, false, Arg.Any<IDictionary<string, object>>())).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Other error"));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.CreateRoute<Customer>(_routeSetupData);
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
            setup.CreateRoute<Customer>(_routeSetupData);
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
            setup.CreateRoute<Customer>(_routeSetupData);
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
            setup.CreateRoute<Customer>(_routeSetupData);
        }

        [TestMethod]
        public void Should_apply_DeadLetter_params()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);
            _routeSetupData.QueueSetupData.DeadLetterExchange = "DeadLetterExchange.Name";
            _routeSetupData.QueueSetupData.DeadLetterRoutingKey = "DeadLetterRoutingKey.Name";

            // Action
            setup.CreateRoute<Customer>(_routeSetupData);

            // Assert
            model.Received(1).QueueDeclare(Arg.Any<string>(), true, false, false, Arg.Is<IDictionary<string, object>>(dic => dic.Count == 4));
        }
    }
}
// ReSharper restore InconsistentNaming