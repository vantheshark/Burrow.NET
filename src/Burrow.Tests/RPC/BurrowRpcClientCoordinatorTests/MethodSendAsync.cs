using Burrow.RPC;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.BurrowRpcClientCoordinatorTests
{
    [TestFixture]
    public class MethodSendAsync
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
        public void Should_publish_request_without_address()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();
            var client = new BurrowRpcClientCoordinator<ISomeService>(null, Substitute.For<IRpcRouteFinder>());

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