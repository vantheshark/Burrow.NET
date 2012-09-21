using System;
using System.Collections;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcServerCoordinatorTests
{
    [TestClass]
    public class MethodStart
    {
        private ITunnel tunnel;

        [TestInitialize]
        public void Setup()
        {
            tunnel = Substitute.For<ITunnel>();
            RabbitTunnel.Factory = Substitute.For<TunnelFactory>();
            RabbitTunnel.Factory.Create(Arg.Any<string>()).Returns(tunnel);
        }

        [TestMethod]
        public void Should_create_tunnel_and_set_serializer_and_route_finder()
        {
            // Arrange
            var routeFinder = Substitute.For<IRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string");

            // Action
            server.Start();

            // Assert
            tunnel.Received(1).SetRouteFinder(routeFinder);
            tunnel.Received(1).SetSerializer(Arg.Any<ISerializer>());
        }

        [TestMethod]
        public void Should_create_balance_request_queue_if_exchange_is_not_empty()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string", "10");
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));

            routeFinder.FindQueueName<RpcRequest>("ISomeService").Returns("ISomeService.RequestQueue");
            routeFinder.FindQueueName<RpcRequest>("ISomeService.10").Returns("ISomeService.10.RequestQueue");
            routeFinder.FindExchangeName<RpcRequest>().Returns("ISomeService.Exchange");

            // Action
            server.Start();

            // Assert
            model.Received(1).QueueDeclare("ISomeService.10.RequestQueue", true, false, true, Arg.Any<IDictionary>());
            model.Received(1).ExchangeDeclare("ISomeService.Exchange", "direct", true, false, null);
            model.Received(1).QueueBind("ISomeService.10.RequestQueue", "ISomeService.Exchange", "ISomeService.RequestQueue");
        }

        [TestMethod]
        public void Should_subscribe_to_request_queue()
        {
            // Arrange
            var routeFinder = Substitute.For<IRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string");

            // Action
            server.Start();

            // Assert
            tunnel.Received(1).SubscribeAsync(Arg.Any<string>(), Arg.Any<Action<RpcRequest>>());
        }
    }
}
// ReSharper restore InconsistentNaming