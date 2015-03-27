using System;
using Burrow.Internal;
using NUnit.Framework;
using Burrow.Tests.Extras.RabbitSetupTests;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.BinarySerializerTests
{
    [TestFixture]
    public class MethodDeserialize
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
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