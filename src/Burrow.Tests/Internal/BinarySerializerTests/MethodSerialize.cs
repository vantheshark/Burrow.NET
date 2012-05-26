using System;
using Burrow.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.BinarySerializerTests
{
    [TestClass]
    public class MethodSerialize
    {
        [TestMethod]
        public void Can_serialize_object()
        {
            // Arrange
            var serializer = new BinarySerializer();


            // Action
            var str = serializer.Serialize(new Customer { FullName = "Bunny", Title = "Mr" });
            Customer obj = serializer.Deserialize<Customer>(str);

            // Asert
            Assert.AreEqual("Bunny", obj.FullName);
            Assert.AreEqual("Mr", obj.Title);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_msg_is_null()
        {
            // Arrange
            var serializer = new BinarySerializer();


            // Action
            var str = serializer.Serialize<Customer>(null);
        }
    }
}
// ReSharper restore InconsistentNaming