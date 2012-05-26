using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DurableConnectionTests
{
    [TestClass]
    public class MethodConnect : DurableConnectionTestHelper
    {
        private readonly AutoResetEvent sync = new AutoResetEvent(true);

        [TestInitialize]
        public void LockThread()
        {
            sync.WaitOne();
            DurableConnection.SharedConnections.Clear();
        }

        [TestCleanup]
        public void ReleaseThread()
        {
            sync.Set();
        }

        [TestMethod]
        public void Should_fire_connected_event_when_connect_successfully()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var connectionFactory = CreateMockConnectionFactory("/");
            var durableConnection = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                          Substitute.For<IRabbitWatcher>(),
                                                          connectionFactory);
            durableConnection.Connected += () => are.Set();

            // Action
            durableConnection.Connect();
            are.WaitOne();

            // Assert
            connectionFactory.Received(1).CreateConnection();
            Assert.AreEqual(1, DurableConnection.SharedConnections.Count);
            Assert.IsTrue(connectionFactory.Endpoint.Equals(DurableConnection.SharedConnections.First().Value.Endpoint));
        }

        [TestMethod]
        public void Should_catch_BrokerUnreachableException()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();

            var connectionFactory = CreateMockConnectionFactory("/");
            connectionFactory.When(x => x.CreateConnection())
                             .Do(callInfo => { throw new BrokerUnreachableException(Substitute.For<IDictionary>(), Substitute.For<IDictionary>()); });
            var durableConnection = new DurableConnection(retryPolicy, Substitute.For<IRabbitWatcher>(), connectionFactory);
            // Action
            durableConnection.Connect();

            // Assert
            retryPolicy.Received(1).WaitForNextRetry(Arg.Any<Action>());
        }

        [TestMethod]
        public void Should_create_only_one_connection_to_the_same_endpoint()
        {
            // Arrange
            var connectionFactory = CreateMockConnectionFactory("/");
            var durableConnection = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                          Substitute.For<IRabbitWatcher>(),
                                                          connectionFactory);

            // Action
            var tasks = new List<Task>();
            for(var i=0; i< 100; i++)
            {
                tasks.Add(Task.Factory.StartNew(durableConnection.Connect));
            }
            Task.WaitAll(tasks.ToArray());


            // Assert
            connectionFactory.Received(1).CreateConnection();
            Assert.AreEqual(1, DurableConnection.SharedConnections.Count);
            Assert.IsTrue(connectionFactory.Endpoint.Equals(DurableConnection.SharedConnections.First().Value.Endpoint));
        }


        [TestMethod]
        public void Should_create_only_one_connection_to_the_each_endpoint()
        {
            // Arrange
            var connectionFactory1 = CreateMockConnectionFactory("/");
            var durableConnection1 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                           Substitute.For<IRabbitWatcher>(),
                                                           connectionFactory1);

            var connectionFactory2 = CreateMockConnectionFactory("/2ndVirtualHost");
            var durableConnection2 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                           Substitute.For<IRabbitWatcher>(),
                                                           connectionFactory2);

            // Action
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                tasks.Add(Task.Factory.StartNew(durableConnection1.Connect));
                tasks.Add(Task.Factory.StartNew(durableConnection2.Connect));
            }
            Task.WaitAll(tasks.ToArray());

            // Assert
            connectionFactory1.Received(1).CreateConnection();
            connectionFactory2.Received(1).CreateConnection();
            Assert.AreEqual(2, DurableConnection.SharedConnections.Count);
        }

        [TestMethod]
        public void Can_create_connections_to_different_endpoints_which_have_the_same_virtualHost()
        {
            // Arrange
            var endpoint1 = new AmqpTcpEndpoint("localhost", 5672);
            var connectionFactory1 = CreateMockConnectionFactory("/", endpoint1);
            var durableConnection1 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                           Substitute.For<IRabbitWatcher>(),
                                                           connectionFactory1);

            var endpoint2 = new AmqpTcpEndpoint("localhost", 5673);
            var connectionFactory2 = CreateMockConnectionFactory("/", endpoint2);
            var durableConnection2 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                           Substitute.For<IRabbitWatcher>(),
                                                           connectionFactory2);

            // Action
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                tasks.Add(Task.Factory.StartNew(durableConnection1.Connect));
                tasks.Add(Task.Factory.StartNew(durableConnection2.Connect));
            }
            Task.WaitAll(tasks.ToArray());

            // Assert
            connectionFactory1.Received(1).CreateConnection();
            connectionFactory2.Received(1).CreateConnection();
            Assert.AreEqual(2, DurableConnection.SharedConnections.Count);
        }
    }
}
// ReSharper restore InconsistentNaming