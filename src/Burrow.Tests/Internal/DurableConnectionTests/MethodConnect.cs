using System;
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
            ManagedConnectionFactory.SharedConnections.Clear();
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
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost1");
            var durableConnection = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                          Substitute.For<IRabbitWatcher>(),
                                                          connectionFactory);
            durableConnection.Connected += () => are.Set();

            // Action
            durableConnection.Connect();
            Assert.IsTrue(are.WaitOne(1000));

            // Assert
            connectionFactory.Received(1).CreateConnection();
            Assert.AreEqual(1, ManagedConnectionFactory.SharedConnections.Count);
            Assert.IsTrue(connectionFactory.Endpoint.Equals(ManagedConnectionFactory.SharedConnections.First().Value.Endpoint));
        }

        [TestMethod]
        public void Should_catch_ConnectFailureException()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();

            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost2");
            connectionFactory.When(x => x.CreateConnection())
                             .Do(callInfo => { throw new ConnectFailureException("Network error", Substitute.For<Exception>()); });
            var durableConnection = new DurableConnection(retryPolicy, Substitute.For<IRabbitWatcher>(), connectionFactory);
            // Action
            durableConnection.Connect();

            // Assert
            retryPolicy.Received(1).WaitForNextRetry(Arg.Any<Action>());
        }

        [TestMethod]
        public void Should_catch_BrokerUnreachableException()
        {
            // Arrange
            var retryPolicy = Substitute.For<IRetryPolicy>();

            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost3");
            connectionFactory.When(x => x.CreateConnection())
                             .Do(callInfo => { throw new BrokerUnreachableException(Substitute.For<Exception>()); });
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
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost4");
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
            Assert.AreEqual(1, ManagedConnectionFactory.SharedConnections.Count);
            Assert.IsTrue(connectionFactory.Endpoint.Equals(ManagedConnectionFactory.SharedConnections.First().Value.Endpoint));
        }


        [TestMethod]
        public void Should_create_only_one_connection_to_the_each_endpoint()
        {
            // Arrange
            var connectionFactory1 = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost5");
            var durableConnection1 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                           Substitute.For<IRabbitWatcher>(),
                                                           connectionFactory1);

            var connectionFactory2 = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost6");
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
            Assert.AreEqual(2, ManagedConnectionFactory.SharedConnections.Count);
        }

        [TestMethod]
        public void Can_create_connections_to_different_endpoints_which_have_the_same_virtualHost()
        {
            // Arrange
            var endpoint1 = new AmqpTcpEndpoint("localhost", 5672);
            var connectionFactory1 = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost7", endpoint1);
            var durableConnection1 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                           Substitute.For<IRabbitWatcher>(),
                                                           connectionFactory1);

            var endpoint2 = new AmqpTcpEndpoint("localhost", 5673);
            var connectionFactory2 = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost8", endpoint2);
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
            Assert.AreEqual(2, ManagedConnectionFactory.SharedConnections.Count);
        }

        [TestMethod]
        public void Should_be_notified_about_a_new_established_connection()
        {
            // Arrange
            var connection2Connected = false;
            var connectionFactory = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost9");
            var durableConnection1 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                          Substitute.For<IRabbitWatcher>(),
                                                          connectionFactory);

            var durableConnection2 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                          Substitute.For<IRabbitWatcher>(),
                                                          connectionFactory);


            durableConnection2.Connected += () => connection2Connected = true;

            // Action
            durableConnection1.Connect();


            // Assert
            connectionFactory.Received(1).CreateConnection();
            Assert.IsTrue(connection2Connected);
        }

        [TestMethod]
        public void Should_only_be_notified_about_a_new_established_connection_that_has_the_same_endpoint_and_virtual_host()
        {
            // Arrange
            var connection1Connected = false;
            var connection2Connected = false;
            var connectionFactory1 = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost10");
            var durableConnection1 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                          Substitute.For<IRabbitWatcher>(),
                                                          connectionFactory1);
            durableConnection1.Connected += () => connection1Connected = true;

            var connectionFactory2 = CreateMockConnectionFactory<ManagedConnectionFactory>("/vHost11");
            var durableConnection2 = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                          Substitute.For<IRabbitWatcher>(),
                                                          connectionFactory2);
            durableConnection2.Connected += () => connection2Connected = true;

            // Action
            durableConnection1.Connect();


            // Assert
            connectionFactory1.Received(1).CreateConnection();
            Assert.IsTrue(connection1Connected);
            Assert.IsFalse(connection2Connected);
        }
    }
}
// ReSharper restore InconsistentNaming