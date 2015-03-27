using System;
using System.Collections.Generic;
using Burrow.RPC;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcClientCoordinatorTests
{
    [TestFixture]
    public class Constructor
    {
        private ITunnel tunnel;

        [SetUp]
        public void Setup()
        {
            tunnel = Substitute.For<ITunnel>();
            RabbitTunnel.Factory = Substitute.For<TunnelFactory>();
            RabbitTunnel.Factory.Create(Arg.Any<string>()).Returns(tunnel);
        }

        [Test]
        public void Can_provide_null_route_finder()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
           
            // Action
            new BurrowRpcClientCoordinator<ISomeService>("host=anyhost;username=guest;password=guest");

            // Assert
            tunnel.Received(1).SetRouteFinder(Arg.Is<IRouteFinder>(arg => arg is RpcRouteFinderAdapter));
        }

        [Test]
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

        [Test]
        public void Should_create_request_and_response_queues_if_provided_true_param()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();

            // Action
            new BurrowRpcClientCoordinator<ISomeService>("host=anyhost;username=guest;password=guest");

            // Assert
            InternalDependencies.RpcQueueHelper.Received(1).CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>());
        }

        [Test]
        public void Should_create_request_and_response_queue_by_default()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));

            routeFinder.RequestQueue.Returns("ISomeService.RequestQueue");
            routeFinder.UniqueResponseQueue.Returns("ISomeService.ResponseQueue");
            routeFinder.RequestExchangeName.Returns("ISomeService.RequestExchange");
            routeFinder.RequestExchangeType.Returns("direct");
            routeFinder.CreateExchangeAndQueue.Returns(true);

            // Action
            new BurrowRpcClientCoordinator<ISomeService>(null, routeFinder);

            // Assert
            model.Received(1).QueueDeclare("ISomeService.ResponseQueue", true, false, true, Arg.Any<IDictionary<string, object>>());
            model.Received(1).QueueDeclare("ISomeService.RequestQueue", true, false, false, Arg.Any<IDictionary<string, object>>());
            model.Received(1).ExchangeDeclare("ISomeService.RequestExchange", "direct", true, false, null);
            model.Received(1).QueueBind("ISomeService.RequestQueue", "ISomeService.RequestExchange", "ISomeService.RequestQueue");
        }
    }
}
// ReSharper restore InconsistentNaming