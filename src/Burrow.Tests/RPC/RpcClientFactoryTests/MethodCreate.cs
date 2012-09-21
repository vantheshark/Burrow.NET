using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.RpcClientFactoryTests
{
    [TestClass]
    public class MethodCreate
    {
        [TestMethod]
        public void Can_accept_null_params()
        {
            // Arrange
            RpcClientFactory.Create<ISomeService>();
        }

        [TestMethod]
        public void Can_accept_null_filters()
        {
            // Arrange
            RpcClientFactory.Create<ISomeService>(NSubstitute.Substitute.For<IRpcClientCoordinator>());
        }
    }
}
// ReSharper restore InconsistentNaming