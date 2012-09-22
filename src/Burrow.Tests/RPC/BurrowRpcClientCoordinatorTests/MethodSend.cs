using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcClientCoordinatorTests
{
    [TestClass]
    public class MethodSend
    {
        private ITunnel tunnel;

        [TestInitialize]
        public void Setup()
        {
            tunnel = Substitute.For<ITunnel>();
            RabbitTunnel.Factory = Substitute.For<TunnelFactory>();
            RabbitTunnel.Factory.Create(Arg.Any<string>()).Returns(tunnel);
            Global.DefaultWatcher = Substitute.For<IRabbitWatcher>();
        }

        [TestMethod]
        public void Should_publish_request_with_address()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            routeFinder.RequestQueue.Returns("RequestQueue");
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            var client = new BurrowRpcClientCoordinator<ISomeService>(null, routeFinder);

            var res = new RpcRequest
            {
                MethodName = "TryParse",
                Id = Guid.NewGuid(),
                UtcExpiryTime = DateTime.UtcNow.AddSeconds(20),
            };

            tunnel.When(x => x.Publish(Arg.Any<RpcRequest>(), Arg.Any<string>()))
                  .Do(callInfo =>
                    {
                        var waithHandler = client.GetCachedWaitHandlers()[res.Id];
                        waithHandler.WaitHandle.Set();
                        waithHandler.Response = new RpcResponse { ChangedParams = new Dictionary<string, object> { { "result", "1000" } } };
                    });
            // Action
            client.Send(res);

            // Assert
            tunnel.Received(1).Publish(Arg.Any<RpcRequest>(), "RequestQueue");
        }

        [TestMethod, ExpectedException(typeof(TimeoutException))]
        public void Should_throw_exeception_if_timeout()
        {
            // Arrange
            var routeFinder = Substitute.For<IRpcRouteFinder>();
            routeFinder.UniqueResponseQueue.Returns("ISomeService.ResponseQueue");
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            var client = new BurrowRpcClientCoordinator<ISomeService>(null, routeFinder);

            var res = new RpcRequest
            {
                UtcExpiryTime = DateTime.UtcNow.AddSeconds(1)
            };

            // Action
            client.Send(res);
        }
    }
}
// ReSharper restore InconsistentNaming