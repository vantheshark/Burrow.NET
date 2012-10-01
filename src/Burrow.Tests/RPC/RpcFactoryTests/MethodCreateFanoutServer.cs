using System;
using System.Collections;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcFactoryTests
{
    [TestClass]
    public class MethodCreateFanoutServer
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
            model.Received(1).QueueDeclare("Burrow.Queue.Rpc.ServerId.ISomeService.Requests", true, false, true, Arg.Any<IDictionary>());
            model.Received(1).ExchangeDeclare("Burrow.Exchange.FANOUT.Rpc.ISomeService.Requests", "fanout", true, false, null);
            tunnel.Received(1).SubscribeAsync("ServerId", Arg.Any<Action<RpcRequest>>());
        }
    }
}
// ReSharper restore InconsistentNaming