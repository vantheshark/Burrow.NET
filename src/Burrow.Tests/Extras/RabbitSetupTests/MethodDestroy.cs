using System;
using Burrow.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.RabbitSetupTests
{
    [TestClass]
    public class MethodDestroy
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
                                     QueueSetupData = new QueueSetupData()
                                 };
        }
        
        [TestMethod]
        public void Should_destroy_exchange_queues()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DestroyRoute<Customer>(_routeSetupData);

            // Assert
            model.Received().QueueDelete("Queue.Customer");
            model.Received().ExchangeDelete("Exchange.Customer");            
        }

        [TestMethod]
        public void Should_not_throw_error_if_cannot_delete_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDelete("Queue.Customer")).Do(callInfo =>
            {
                throw new Exception("Test Exception");
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DestroyRoute<Customer>(_routeSetupData);

            // Assert
            model.Received().ExchangeDelete("Exchange.Customer");
        }

        [TestMethod]
        public void Should_not_throw_error_if_cannot_delete_exchange()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.ExchangeDelete("Exchange.Customer")).Do(callInfo =>
            {
                throw new Exception("Test Exception");
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DestroyRoute<Customer>(_routeSetupData);
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_when_trying_to_delete_none_exist_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDelete("Queue.Customer")).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "NOT_FOUND - no queue "));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DestroyRoute<Customer>(_routeSetupData);
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_and_log_error_when_trying_to_delete_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.QueueDelete("Queue.Customer")).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Other error"));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DestroyRoute<Customer>(_routeSetupData);
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_when_trying_to_delete_none_exist_exchange()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.ExchangeDelete("Exchange.Customer")).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "NOT_FOUND - no exchange "));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DestroyRoute<Customer>(_routeSetupData);
        }

        [TestMethod]
        public void Should_catch_OperationInterruptedException_and_log_error_when_trying_to_delete_exchange()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            model.When(x => x.ExchangeDelete("Exchange.Customer")).Do(callInfo =>
            {
                throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Other error"));
            });
            var setup = RabbitSetupForTest.CreateRabbitSetup(model);

            // Action
            setup.DestroyRoute<Customer>(_routeSetupData);
        }
    }
}
// ReSharper restore InconsistentNaming