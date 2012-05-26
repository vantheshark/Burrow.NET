using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.TypeNameSerializerTests
{
    [TestClass]
    public class MethodSerialize
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_type_is_null()
        {
            // Arrange
            var serializer = new TypeNameSerializer();

            // Action
            serializer.Serialize(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
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