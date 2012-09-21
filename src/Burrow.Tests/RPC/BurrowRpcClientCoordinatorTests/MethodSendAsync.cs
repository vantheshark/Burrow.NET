using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcClientCoordinatorTests
{
    [TestClass]
    public class MethodSendAsync
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
        public void Should_publish_request_without_address()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            var client = new BurrowRpcClientCoordinator<ISomeService>(null, Substitute.For<IRouteFinder>());

            var res = new RpcRequest
            {
                ResponseAddress = "abc"
                
            };

            // Action
            client.SendAsync(res);

            // Assert
            tunnel.Received(1).Publish(Arg.Any<RpcRequest>(), Arg.Any<string>());
            Assert.IsNull(res.ResponseAddress);
            

        }
    }
}
// ReSharper restore InconsistentNaming