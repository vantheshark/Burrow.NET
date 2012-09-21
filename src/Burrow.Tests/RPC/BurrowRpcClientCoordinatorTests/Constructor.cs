using System;
using System.Collections;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcClientCoordinatorTests
{
    [TestClass]
    public class Constructor
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
        public void Can_provide_null_route_finder()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
           
            // Action
            new BurrowRpcClientCoordinator<ISomeService>("host=anyhost;username=guest;password=guest");

            // Assert
            tunnel.Received(1).SetRouteFinder(Arg.Is<IRouteFinder>(arg => arg is BurrowRpcRouteFinder));
        }

        [TestMethod]
        public void Should_set_default_serializer()
        {
            // Arrange
            var serializer = Substitute.For<ISerializer>();
            var oldSerializer = Global.DefaultSerializer;
            Global.DefaultSerializer = serializer;
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();

            // Action
            new BurrowRpcClientCoordinator<ISomeService>("host=anyhost;username=guest;password=guest");

            // Assert
            tunnel.Received(1).SetSerializer(serializer);
            Global.DefaultSerializer = oldSerializer;
        }

        [TestMethod]
        public void Should_create_request_and_response_queues_if_provided_true_param()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();

            // Action
            new BurrowRpcClientCoordinator<ISomeService>("host=anyhost;username=guest;password=guest");

            // Assert
            InternalDependencies.RpcQueueHelper.Received(1).CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>());
        }

        [TestMethod]
        public void Should_create_request_and_response_queue_by_default()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRouteFinder>();
            
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));

            routeFinder.FindQueueName<RpcRequest>(typeof(ISomeService).Name).Returns("ISomeService.RequestQueue");
            routeFinder.FindQueueName<RpcResponse>(Arg.Any<string>()).Returns("ISomeService.ResponseQueue");
            routeFinder.FindExchangeName<RpcRequest>().Returns("ISomeService.RequestExchange");

            // Action
            new BurrowRpcClientCoordinator<ISomeService>(null, routeFinder);

            // Assert
            model.Received(1).QueueDeclare("ISomeService.ResponseQueue", true, false, true, Arg.Any<IDictionary>());
            model.Received(1).QueueDeclare("ISomeService.RequestQueue", true, false, false, Arg.Any<IDictionary>());
            model.Received(1).ExchangeDeclare("ISomeService.RequestExchange", "direct", true, false, null);
            model.Received(1).QueueBind("ISomeService.RequestQueue", "ISomeService.RequestExchange", "ISomeService.RequestQueue");
        }
    }
}
// ReSharper restore InconsistentNaming