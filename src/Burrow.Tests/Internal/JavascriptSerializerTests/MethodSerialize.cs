using Burrow.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.JavascriptSerializerTests
{
    [TestClass]
    public class MethodSerialize
    {
        [TestMethod]
        public void Can_serialize_object()
        {
            // Arrange
            var serializer = new JavaScriptSerializer();


            // Action
            var str = serializer.Serialize(new Customer { FullName = "Bunny", Title = "Mr" });
            Customer obj = serializer.Deserialize<Customer>(str);

            // Asert
            Assert.AreEqual("Bunny", obj.FullName);
            Assert.AreEqual("Mr", obj.Title);
        }

        [TestMethod]
        public void Should_return_null_if_msg_is_null()
        {
            // Arrange
            var serializer = new JavaScriptSerializer();


            // Action
            var str = serializer.Serialize<Customer>(null);

            // Asert
            Assert.IsNull(str);
        }
    }
}
// ReSharper restore InconsistentNaming