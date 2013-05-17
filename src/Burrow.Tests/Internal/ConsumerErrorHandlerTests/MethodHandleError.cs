using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Burrow.Internal;
using Burrow.Tests.Internal.DurableConnectionTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsumerErrorHandlerTests
{
    [TestClass]
    public class MethodHandleError : DurableConnectionTestHelper
    {
        [TestMethod]
        public void Should_create_exchange_queue_and_put_error_to_queue ()
        {
            // Arrange
            IConnection connection;
            var connectionFactory = CreateMockConnectionFactory("/", out connection);
            var model = Substitute.For<IModel>();
            connection.CreateModel().Returns(model);
            var basicProperies = Substitute.For<IBasicProperties>();
            model.CreateBasicProperties().Returns(basicProperies);

            var handler = new ConsumerErrorHandler(connectionFactory, Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());

            // Action
            handler.HandleError(new BasicDeliverEventArgs{Body = new byte[0], BasicProperties = basicProperies}, new Exception());

            // Assert
            model.Received().BasicPublish(Arg.Any<string>(), string.Empty, basicProperies, Arg.Any<byte[]>());
        }

        [TestMethod]
        public void Should_should_catch_BrokerUnreachableException()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection connection;
            var connectionFactory = CreateMockConnectionFactory("/", out connection);
            connection.When(x => x.CreateModel())
                      .Do(callInfo => { throw new BrokerUnreachableException(Substitute.For<IDictionary>(), Substitute.For<IDictionary>(), Substitute.For<Exception>()); });
            var handler = new ConsumerErrorHandler(connectionFactory, Substitute.For<ISerializer>(), watcher);

            // Action
            handler.HandleError(new BasicDeliverEventArgs { Body = new byte[0] }, new Exception());

            // Assert
            watcher.Received().ErrorFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [TestMethod]
        public void Should_should_catch_OperationInterruptedException()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection connection;
            var connectionFactory = CreateMockConnectionFactory("/", out connection);
            connection.When(x => x.CreateModel())
                      .Do(callInfo => { throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown ;)"));});
            var handler = new ConsumerErrorHandler(connectionFactory, Substitute.For<ISerializer>(), watcher);

            // Action
            handler.HandleError(new BasicDeliverEventArgs { Body = new byte[0] }, new Exception());

            // Assert
            watcher.Received().ErrorFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [TestMethod]
        public void Should_should_catch_other_exceptions()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection connection;
            var connectionFactory = CreateMockConnectionFactory("/", out connection);
            connection.When(x => x.CreateModel())
                      .Do(callInfo => { throw new Exception("unexpecctedException"); });
            var handler = new ConsumerErrorHandler(connectionFactory, Substitute.For<ISerializer>(), watcher);

            // Action
            handler.HandleError(new BasicDeliverEventArgs { Body = new byte[0] }, new Exception());

            // Assert
            watcher.Received().ErrorFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}
// ReSharper restore InconsistentNaming