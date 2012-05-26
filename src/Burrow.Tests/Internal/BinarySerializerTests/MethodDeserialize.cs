using System;
using Burrow.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.BinarySerializerTests
{
    [TestClass]
    public class MethodDeserialize
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_byte_array_is_null()
        {
            // Arrange
            var serializer = new BinarySerializer();


            // Action
            var obj = serializer.Deserialize<Customer>(null);
        }
    }
}
// ReSharper restore InconsistentNaming