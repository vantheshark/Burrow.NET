using System;
using Burrow.Internal;
using NUnit.Framework;
using Burrow.Tests.Extras.RabbitSetupTests;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.BinarySerializerTests
{
    [TestFixture]
    public class MethodSerialize
    {
        [Test]
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

        [Test, ExpectedException(typeof(ArgumentNullException))]
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