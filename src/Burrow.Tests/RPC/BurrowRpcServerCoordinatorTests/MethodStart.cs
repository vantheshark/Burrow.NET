using System;
using System.Collections;
using System.Collections.Generic;
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
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string");

            // Action
            server.Start();

            // Assert
            tunnel.Received(1).SetRouteFinder(Arg.Any<IRouteFinder>());
            tunnel.Received(1).SetSerializer(Arg.Any<ISerializer>());
        }

        [TestMethod]
        public void Should_create_exchange_and_auto_delete_request_queue_if_provide_serverId_and_exchange_not_empty()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string", "serverId");
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));

            routeFinder.RequestQueue.Returns("ISomeService.serverId.RequestQueue");
            routeFinder.RequestExchangeType.Returns("direct");
            routeFinder.RequestExchangeName.Returns("ISomeService.Exchange");
            routeFinder.CreateExchangeAndQueue.Returns(true);

            // Action
            server.Start();

            // Assert
            model.Received(1).QueueDeclare("ISomeService.serverId.RequestQueue", true, false, true, Arg.Any<IDictionary<string, object>>());
            model.Received(1).ExchangeDeclare("ISomeService.Exchange", "direct", true, false, null);
            model.Received(1).QueueBind("ISomeService.serverId.RequestQueue", "ISomeService.Exchange", "ISomeService.serverId.RequestQueue");
        }

        [TestMethod]
        public void Should_create_durable_request_queue_if_not_provide_server_id()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string");
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));

            routeFinder.RequestQueue.Returns("ISomeService.serverId.RequestQueue");
            routeFinder.RequestExchangeType.Returns("direct");
            routeFinder.RequestExchangeName.Returns("ISomeService.Exchange");
            routeFinder.CreateExchangeAndQueue.Returns(true);

            // Action
            server.Start();

            // Assert
            model.Received(1).QueueDeclare("ISomeService.serverId.RequestQueue", true, false, false, Arg.Any<IDictionary<string, object>>());
        }

        [TestMethod]
        public void Should_create_durable_request_queue_if_provide_server_id_but_exchange_name_is_null()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            var instance = Substitute.For<ISomeService>();
            var server = new BurrowRpcServerCoordinator<ISomeService>(instance, routeFinder, "queue-connnection-string", "serverId");
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));

            routeFinder.RequestQueue.Returns("ISomeService.serverId.RequestQueue");
            routeFinder.CreateExchangeAndQueue.Returns(true);

            // Action
            server.Start();

            // Assert
            model.Received(1).QueueDeclare("ISomeService.serverId.RequestQueue", true, false, false, Arg.Any<IDictionary<string, object>>());
        }

        [TestMethod]
        public void Should_subscribe_to_request_queue()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
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