using System;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestClass]
    public class MethodDispose : DurableConnectionTestHelper
    {
        [TestMethod]
        public void Should_NOT_close_any_connection()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory("/", out rmqConnection);
            var channel = Substitute.For<IModel>();
            rmqConnection.CreateModel().Returns(channel);

            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            durableConnection.CreateChannel();
            Assert.AreEqual(1, DurableConnection.SharedConnections.Count);


            // Action
            durableConnection.Dispose();

            //Assert
            rmqConnection.DidNotReceive().Close(Arg.Any<ushort>(), Arg.Any<string>());
            rmqConnection.DidNotReceive().Dispose();
            Assert.AreEqual(1, DurableConnection.SharedConnections.Count);
        }

        [TestMethod, Ignore/* Since we don't dispose connections to rabbitmq here*/]
        public void Should_catch_all_exceptions()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();
            var watcher = Substitute.For<IRabbitWatcher>();
            IConnection rmqConnection;
            var connectionFactory = CreateMockConnectionFactory("/", out rmqConnection);
            var channel = Substitute.For<IModel>();
            rmqConnection.CreateModel().Returns(channel);
            rmqConnection.When(x => x.Dispose())
                         .Do(callInfo => { throw new Exception("Can't dispose :D");});
            var durableConnection = new DurableConnection(retryPolicy, watcher, connectionFactory);
            durableConnection.CreateChannel();
            Assert.AreEqual(1, DurableConnection.SharedConnections.Count);


            // Action
            durableConnection.Dispose();

            //Assert
            watcher.Received().Error(Arg.Is<Exception>(x => x.Message == "Can't dispose :D"));
        }
    }
}
// ReSharper restore InconsistentNaming