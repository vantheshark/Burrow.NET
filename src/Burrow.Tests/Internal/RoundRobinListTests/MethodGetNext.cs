using Burrow.Internal;
using NUnit.Framework;
using Burrow.Tests.Extras.RabbitSetupTests;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.RoundRobinListTests
{
    [TestFixture]
    public class MethodGetNext
    {
        [Test]
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