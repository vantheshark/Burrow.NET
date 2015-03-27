using System;
using Burrow.Internal;
using Burrow.Tests.Internal.DurableConnectionTests;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsumerErrorHandlerTests
{
    [TestFixture]
    public class MethodHandleError : DurableConnectionTestHelper
    {
        [Test]
        public void Should_create_exchange_queue_and_put_error_to_queue ()
        {
            // Arrange
            var durableConnection = Substitute.For<IDurableConnection>();

            var model = Substitute.For<IModel>();
            durableConnection.CreateChannel().Returns(model);
            var basicProperies = Substitute.For<IBasicProperties>();
            model.CreateBasicProperties().Returns(basicProperies);

            var handler = new ConsumerErrorHandler(durableConnection, Substitute.For<ISerializer>(), Substitute.For<IRabbitWatcher>());

            // Action
            handler.HandleError(new BasicDeliverEventArgs{Body = new byte[0], BasicProperties = basicProperies}, new Exception());

            // Assert
            model.Received().BasicPublish(Arg.Any<string>(), string.Empty, basicProperies, Arg.Any<byte[]>());
        }

        [Test]
        public void Should_catch_BrokerUnreachableException()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.When(x => x.CreateChannel())
                      .Do(callInfo => { throw new BrokerUnreachableException(Substitute.For<Exception>()); });
            var handler = new ConsumerErrorHandler(durableConnection, Substitute.For<ISerializer>(), watcher);

            // Action
            handler.HandleError(new BasicDeliverEventArgs { Body = new byte[0] }, new Exception());

            // Assert
            watcher.Received().ErrorFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void Should_catch_OperationInterruptedException()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.When(x => x.CreateChannel())
                      .Do(callInfo => { throw new OperationInterruptedException(new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Shutdown ;)"));});
            var handler = new ConsumerErrorHandler(durableConnection, Substitute.For<ISerializer>(), watcher);

            // Action
            handler.HandleError(new BasicDeliverEventArgs { Body = new byte[0] }, new Exception());

            // Assert
            watcher.Received().ErrorFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void Should_catch_other_exceptions()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.When(x => x.CreateChannel())
                      .Do(callInfo => { throw new Exception("unexpecctedException"); });
            var handler = new ConsumerErrorHandler(durableConnection, Substitute.For<ISerializer>(), watcher);

            // Action
            handler.HandleError(new BasicDeliverEventArgs { Body = new byte[0] }, new Exception());

            // Assert
            watcher.Received().ErrorFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void Should_catch_ConnectFailureException_exceptions()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            var durableConnection = Substitute.For<IDurableConnection>();
            durableConnection.When(x => x.CreateChannel())
                      .Do(callInfo => { throw new ConnectFailureException("Connect Failure", Substitute.For<Exception>()); });
            var handler = new ConsumerErrorHandler(Substitute.For<IDurableConnection>(), Substitute.For<ISerializer>(), watcher);

            // Action
            handler.HandleError(new BasicDeliverEventArgs { Body = new byte[0] }, new Exception());

            // Assert
            watcher.Received().ErrorFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}
// ReSharper restore InconsistentNaming