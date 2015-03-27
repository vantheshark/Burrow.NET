using System;
using System.Threading.Tasks;
using Burrow.RPC;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcClientCoordinatorTests
{
    [TestFixture]
    public class MethodReceiveResponse
    {
        private ITunnel tunnel;

        [SetUp]
        public void Setup()
        {
            tunnel = Substitute.For<ITunnel>();
            RabbitTunnel.Factory = Substitute.For<TunnelFactory>();
            RabbitTunnel.Factory.Create(Arg.Any<string>()).Returns(tunnel);
            Global.DefaultWatcher = Substitute.For<IRabbitWatcher>();
        }

        [Test]
        public void Should_return_and_log_warn_msg_if_the_waitHandlers_does_not_contain_requestId()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            var client = new BurrowRpcClientCoordinator<ISomeService>(null, Substitute.For<IRpcRouteFinder>());

            var res = new RpcResponse
            {
                RequestId = Guid.NewGuid()
            };

            // Action
            client.ReceiveResponse(res);

            // Assert
            Global.DefaultWatcher.Received(1).WarnFormat(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void Should_set_respones_value_to_wait_handler_and_set_the_wait_handler()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            var client = new BurrowRpcClientCoordinator<ISomeService>(null, Substitute.For<IRpcRouteFinder>());

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
            Assert.IsTrue(wait.WaitHandle.WaitOne(1000));
            Assert.AreEqual(res, wait.Response);
        }
    }
}
// ReSharper restore InconsistentNaming