using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.HaConnectionTests
{
    [TestClass]
    public class MethodConnect 
    {
        [TestInitialize]
        public void Initialize()
        {
            ManagedConnectionFactory.SharedConnections.Clear();            
        }

        [TestMethod]
        public void Should_fire_connected_event_when_connect_successfully()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var retryPolicty = Substitute.For<IRetryPolicy>();
            var connection = new HaConnection(retryPolicty,
                                              Substitute.For<IRabbitWatcher>(),
                                              new List<ManagedConnectionFactory> { new ManagedConnectionFactory() });
            connection.Connected += () => are.Set();
            connection.ConnectionFactories.ClearAll();
            var f1 = CreateManagedConnectionFactory(5671);
            var f2 = CreateManagedConnectionFactory(5672);
            connection.ConnectionFactories.Add(f1);
            connection.ConnectionFactories.Add(f2);

            // Action
            connection.Connect();
            are.WaitOne();

            // Assert
            retryPolicty.Received(1).Reset();
            f1.Received(1).CreateConnection();
        }

        [TestMethod]
        public void Should_fail_over_to_next_node_if_there_is_ConnectionFailureException()
        {
            // Arrange
            var retryPolicty = Substitute.For<IRetryPolicy>();
            var connection = new HaConnection(retryPolicty,
                                              Substitute.For<IRabbitWatcher>(),
                                              new List<ManagedConnectionFactory> { new ManagedConnectionFactory() });

            connection.ConnectionFactories.ClearAll();
            var f1 = CreateManagedConnectionFactory(5671, new Exception());
            var f2 = CreateManagedConnectionFactory(5672, new Exception());
            var f3 = CreateManagedConnectionFactory(5673, new BrokerUnreachableException(new Dictionary<AmqpTcpEndpoint, int>() , new Dictionary<AmqpTcpEndpoint, Exception>(), new Exception()));
            connection.ConnectionFactories.Add(f1);
            connection.ConnectionFactories.Add(f2);
            connection.ConnectionFactories.Add(f3);

            // Action
            connection.Connect();

            // Assert
            f1.Received(1).CreateConnection();
            f2.Received(1).CreateConnection();
            f3.Received(1).CreateConnection();
            retryPolicty.Received(1).WaitForNextRetry(Arg.Any<Action>());

        }


        [TestMethod]
        public void Should_create_only_one_connection_to_the_each_endpoint()
        {
            // Arrange
            var retryPolicty = Substitute.For<IRetryPolicy>();
            var connection1 = new HaConnection(retryPolicty,
                                              Substitute.For<IRabbitWatcher>(),
                                              new List<ManagedConnectionFactory> { new ManagedConnectionFactory() });

            connection1.ConnectionFactories.ClearAll();
            var f1 = CreateManagedConnectionFactory(5671);
            var f2 = CreateManagedConnectionFactory(5672);
            var f3 = CreateManagedConnectionFactory(5673);
            connection1.ConnectionFactories.Add(f1);
            connection1.ConnectionFactories.Add(f2);
            connection1.ConnectionFactories.Add(f3);

            var connection2 = new HaConnection(retryPolicty,
                                              Substitute.For<IRabbitWatcher>(),
                                              new List<ManagedConnectionFactory> { new ManagedConnectionFactory() });

            connection2.ConnectionFactories.ClearAll();
            var f4 = CreateManagedConnectionFactory(5674);
            var f5 = CreateManagedConnectionFactory(5675);
            var f6 = CreateManagedConnectionFactory(5676);
            connection2.ConnectionFactories.Add(f4);
            connection2.ConnectionFactories.Add(f5);
            connection2.ConnectionFactories.Add(f6);


            // Action
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                tasks.Add(Task.Factory.StartNew(connection1.Connect));
                tasks.Add(Task.Factory.StartNew(connection2.Connect));
            }
            Task.WaitAll(tasks.ToArray());

            // Assert
            f1.Received(1).CreateConnection();
            f4.Received(1).CreateConnection();
            Assert.AreEqual(2, ManagedConnectionFactory.SharedConnections.Count);
        }

        private ManagedConnectionFactory CreateManagedConnectionFactory(int port, Exception exception = null)
        {
            var factory = Substitute.For<ManagedConnectionFactory>();
            factory.HostName = "rabbitmq-host.com";
            factory.Port = port;
            factory.UserName = "username";
            factory.Password = "password";

            if (exception == null)
            {
                var connection = Substitute.For<IConnection>();
                connection.IsOpen.Returns(true);
                connection.Endpoint.Returns(new AmqpTcpEndpoint
                {
                    HostName = factory.HostName,
                    Port = port,
                    Protocol = factory.Protocol,
                    Ssl = factory.Ssl
                });
                factory.EstablishConnection().Returns(connection);
            }
            else
            {
                factory.When(x => x.CreateConnection())
                       .Do(callInfo =>
                       {
                           if (exception is ConnectFailureException) throw exception;
                           if (exception is BrokerUnreachableException) throw exception;
                           throw new ConnectFailureException("Cannot connect to host", exception);
                       });
            }
            return factory;
        }

        [TestMethod]
        public void Should_be_notified_about_a_new_established_connection()
        {
            // Arrange
            var haConnectionEstablished = false;
            var retryPolicty = Substitute.For<IRetryPolicy>();
            var haConnection = new HaConnection(retryPolicty,
                                              Substitute.For<IRabbitWatcher>(),
                                              new List<ManagedConnectionFactory> { new ManagedConnectionFactory() });

            haConnection.ConnectionFactories.ClearAll();
            var f1 = CreateManagedConnectionFactory(5671);
            var f2 = CreateManagedConnectionFactory(5672);
            var f3 = CreateManagedConnectionFactory(5673);
            haConnection.ConnectionFactories.Add(f1);
            haConnection.ConnectionFactories.Add(f2);
            haConnection.ConnectionFactories.Add(f3);

            var durableConnection = new DurableConnection(Substitute.For<IRetryPolicy>(),
                                                          Substitute.For<IRabbitWatcher>(),
                                                          CreateManagedConnectionFactory(5671));

            haConnection.Connected += () => haConnectionEstablished = true;
            

            // Action
            durableConnection.Connect();


            // Assert
            Assert.IsTrue(haConnectionEstablished);
        }
    }
}
// ReSharper restore InconsistentNaming