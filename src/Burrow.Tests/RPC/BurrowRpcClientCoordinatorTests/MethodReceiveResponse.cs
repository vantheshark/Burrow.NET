using System;
using System.Threading.Tasks;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcClientCoordinatorTests
{
    [TestClass]
    public class MethodReceiveResponse
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
        public void Should_return_and_log_warn_msg_if_the_waitHandlers_does_not_contain_requestId()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            var client = new BurrowRpcClientCoordinator<ISomeService>(null, Substitute.For<IRouteFinder>());

            var res = new RpcResponse
            {
                RequestId = Guid.NewGuid()
            };

            // Action
            client.ReceiveResponse(res);

            // Assert
            Global.DefaultWatcher.Received(1).WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [TestMethod]
        public void Should_set_respones_value_to_wait_handler_and_set_the_wait_handler()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            var client = new BurrowRpcClientCoordinator<ISomeService>(null, Substitute.For<IRouteFinder>());

            var res = new RpcResponse
            {
                RequestId = Guid.NewGuid()
            };
            var handlers = client.GetCachedWaitHandlers();
            var wait = new RpcWaitHandler();
            handlers.TryAdd(res.RequestId, wait);

            // Action
            Task.Factory.StartNew(() => client.ReceiveResponse(res));

            // Assert
            wait.WaitHandle.WaitOne();
            Assert.AreEqual(res, wait.Response);
        }
    }
}
// ReSharper restore InconsistentNaming