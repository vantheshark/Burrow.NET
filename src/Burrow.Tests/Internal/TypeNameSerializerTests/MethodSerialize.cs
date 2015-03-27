using System;
using Burrow.Internal;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.TypeNameSerializerTests
{
    [TestFixture]
    public class MethodSerialize
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_type_is_null()
        {
            // Arrange
            var serializer = new TypeNameSerializer();

            // Action
            serializer.Serialize(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_type_fullName_is_null()
        {
            // Arrange
            var type = Substitute.For<Type>();
            type.FullName.Returns((string)null);
            var serializer = new TypeNameSerializer();

            // Action
            serializer.Serialize(type);
        }
    }
}
// ReSharper restore InconsistentNaming