using System;
using System.Collections.Generic;
using System.Threading;
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
            var f1 = CreateManagedConnectionFactory(5671, false);
            var f2 = CreateManagedConnectionFactory(5672, false);
            var f3 = CreateManagedConnectionFactory(5673, false);
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

        private ManagedConnectionFactory CreateManagedConnectionFactory(int port, bool connectable = true)
        {
            var factory = Substitute.For<ManagedConnectionFactory>();
            factory.HostName = "rabbitmq-host.com";
            factory.Port = port;
            factory.UserName = "username";
            factory.Password = "password";
            
            if (connectable)
            {
                var connection = Substitute.For<IConnection>();
                connection.IsOpen.Returns(true);
                factory.CreateConnection().Returns(connection);
            }
            else
            {
                factory.When(x => x.CreateConnection())
                       .Do(callInfo =>
                       {
                           throw new ConnectFailureException("Host is not reachable!", new Exception());
                       });
            }
            return factory;
        }
    }
}
// ReSharper restore InconsistentNaming