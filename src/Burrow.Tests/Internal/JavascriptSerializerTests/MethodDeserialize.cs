using Burrow.Internal;
using NUnit.Framework;
using Burrow.Tests.Extras.RabbitSetupTests;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.JavascriptSerializerTests
{
    [TestFixture]
    public class MethodDeserialize
    {
        [Test]
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