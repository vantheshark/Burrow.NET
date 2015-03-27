using Burrow.RPC;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcFactoryTests
{
    [TestFixture]
    public class MethodCreateClient
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
        public void Can_accept_null_params()
        {
            // Arrange
            InternalDependencies.RpcQueueHelper = Substitute.For<IRpcQueueHelper>();


            // Action
            RpcFactory.CreateClient<ISomeService>();
        }

        [Test]
        public void Can_accept_null_filters()
        {
            // Arrange
            RpcFactory.CreateClient<ISomeService>(Substitute.For<IRpcClientCoordinator>());
        }
    }
}
// ReSharper restore InconsistentNaming