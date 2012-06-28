using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.ByteArrayStringConverterTests
{
    [TestClass]
    public class MethodToString
    {
        [TestMethod]
        public void Should_convert_to_string()
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes("Burrow.NET is awesome");

            // Action
            var result = ByteArrayStringConverter.ToString(byteArray);

            // Assert
            Assert.AreEqual("Burrow.NET is awesome", result);

        }


        [TestMethod]
        public void Should_be_able_to_convert_big_byte_array_to_string()
        {
            // Arrange
            var size = 200000000; //100MB
            var byteArray = new byte[size];

            for(int i=0; i< size; i++)
            {
                byteArray[i] =  (byte)(i%byte.MaxValue);
            }

            var result = Encoding.UTF8.GetString(byteArray);

            // Action
            result = ByteArrayStringConverter.ToString(byteArray);

            // Assert
            Assert.AreEqual(size, result.Length);

        }
    }
}
// ReSharper restore InconsistentNaming