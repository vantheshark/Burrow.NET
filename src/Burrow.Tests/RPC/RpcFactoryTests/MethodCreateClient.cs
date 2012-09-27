using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcFactoryTests
{
    [TestClass]
    public class MethodCreateClient
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
        public void Can_accept_null_params()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();


            // Action
            RpcFactory.CreateClient<ISomeService>();
        }

        [TestMethod]
        public void Can_accept_null_filters()
        {
            // Arrange
            RpcFactory.CreateClient<ISomeService>(Substitute.For<IRpcClientCoordinator>());
        }
    }
}
// ReSharper restore InconsistentNaming