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
    public class MethodCreateFanoutServer
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
        public void Should_use_DefaultFanoutRpcRequestRouteFinder()
        {
            // Arrange
           var model = Substitute.For<IModel>();
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            InternalDependencies.RpcQueueHelper
                .When(x => x.CreateQueues(Arg.Any<string>(), Arg.Any<Action<IModel>>()))
                .Do(callInfo => callInfo.Arg<Action<IModel>>()(model));


            // Action
            var server = RpcFactory.CreateFanoutServer(Substitute.For<ISomeService>(), serverId: "ServerId") as BurrowRpcServerCoordinator<ISomeService>;
            Assert.IsNotNull(server);
            server.Start();

            // Assert
            model.Received(1).QueueDeclare("Burrow.Queue.Rpc.ServerId.ISomeService.Requests", true, false, true, Arg.Any<IDictionary<string, object>>());
            model.Received(1).ExchangeDeclare("Burrow.Exchange.FANOUT.Rpc.ISomeService.Requests", "fanout", true, false, null);
            tunnel.Received(1).SubscribeAsync("ServerId", Arg.Any<Action<RpcRequest>>());
        }
    }
}
// ReSharper restore InconsistentNaming