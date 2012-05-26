using Burrow.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.JavascriptSerializerTests
{
    [TestClass]
    public class MethodDeserialize
    {
        [TestMethod]
        public void Should_return_null_if_byte_array_is_null()
        {
            // Arrange
            var serializer = new JavaScriptSerializer();


            // Action
            var obj = serializer.Deserialize<Customer>(null);

            // Assert
            Assert.IsNull(obj);
        }
    }
}
// ReSharper restore InconsistentNaming