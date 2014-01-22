using Burrow.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.RoundRobinListTests
{
    [TestClass]
    public class MethodGetNext
    {
        [TestMethod]
        public void Should_return_default_object_if_list_is_empty()
        {
            // Arrange
            var list = new RoundRobinList<Customer>();

            // Action
            var item = list.GetNext();

            // Assert
            Assert.IsNull(item);
        }
    }
}
// ReSharper restore InconsistentNaming