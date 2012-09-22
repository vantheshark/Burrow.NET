using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcFactoryTests
{
    [TestClass]
    public class MethodCreateClient
    {
        [TestMethod]
        public void Can_accept_null_params()
        {
            // Arrange
            RpcFactory.CreateClient<ISomeService>();
        }

        [TestMethod]
        public void Can_accept_null_filters()
        {
            // Arrange
            RpcFactory.CreateClient<ISomeService>(NSubstitute.Substitute.For<IRpcClientCoordinator>());
        }
    }
}
// ReSharper restore InconsistentNaming