using System;
using System.Collections.Generic;
using Burrow.RPC;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcFactoryTests
{
    [TestFixture]
    public class MethodCreateServer
    {
        private ITunnel tunnel;

        [SetUp]
        public void Setup()
        {
            tunnel = Substitute.For<ITunnel>();
            RabbitTunnel.Factory = Substitute.For<TunnelFactory>();
            RabbitTunnel.Factory.Create(Arg.Any<string>()).Returns(tunnel);
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
        }

        [Test]
        public void Can_accept_null_params()
        {
            // Action
            var server = RpcFactory.CreateServer(Substitute.For<ISomeService>()) as BurrowRpcServerCoordinator<ISomeService>;

            // Assert
            Assert.IsNotNull(server);
        }

        [Test]
        public void Can_provide_request_queue_name()
        {
            // Action
            var server = RpcFactory.CreateServer(Substitute.For<ISomeService>(), "RequestQueue") as BurrowRpcServerCoordinator<ISomeService>;

            // Assert
            Assert.IsNotNull(server);
        }

        [Test]
        public void Should_use_DefaulRpcRouteFinder_if_not_provide_serverId()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));

            // Action

            var server = RpcFactory.CreateServer(Substitute.For<ISomeService>()) as BurrowRpcServerCoordinator<ISomeService>;
            Assert.IsNotNull(server);
            server.Start();

            // Assert
            model.Received(1).QueueDeclare("Burrow.Queue.Rpc.ISomeService.Requests", true, false, false, Arg.Any<IDictionary<string, object>>());
            model.DidNotReceive().ExchangeDeclare(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IDictionary<string, object>>());
            tunnel.Received(1).SubscribeAsync(typeof(ISomeService).Name, Arg.Any<Action<RpcRequest>>());
        }

        [Test]
        public void Should_use_DefaulRpcRouteFinder_and_declare_durable_request_queue()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));


            // Action
            var server = RpcFactory.CreateServer(Substitute.For<ISomeService>(), "RequestQueue", serverId: "ServerId") as BurrowRpcServerCoordinator<ISomeService>;
            Assert.IsNotNull(server);
            server.Start();

            // Assert
            model.Received().QueueDeclare("RequestQueue", true, false, false, Arg.Any<IDictionary<string, object>>());
            model.DidNotReceiveWithAnyArgs().ExchangeDeclare("", "", false, false, null);
            tunnel.Received(1).SubscribeAsync("ServerId", Arg.Any<Action<RpcRequest>>());
        }

        [Test]
        public void Should_use_DefaulRpcRouteFinder_and_create_durable_queue_even_if_not_provide_serverId()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));


            // Action
            var server = RpcFactory.CreateServer(Substitute.For<ISomeService>(), "RequestQueue") as BurrowRpcServerCoordinator<ISomeService>;
            Assert.IsNotNull(server);
            server.Start();

            // Assert
            model.Received(1).QueueDeclare("RequestQueue", true, false, false, Arg.Any<IDictionary<string, object>>());
        }
    }
}
// ReSharper restore InconsistentNaming