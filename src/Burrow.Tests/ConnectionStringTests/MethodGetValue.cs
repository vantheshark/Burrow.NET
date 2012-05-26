using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.ConnectionStringTests
{
    [TestClass]
    public class MethodGetValue
    {
        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_try_to_get_value_not_in_connectionstring()
        {
            // Arrange
            var con = new ConnectionString("host=localhost;username=guest;password=guest");

            // Action
            var port = con.GetValue("port");
        }

        [TestMethod]
        public void Should_return_value_provided_in_connectionstring()
        {
            // Arrange
            var con = new ConnectionString("host=localhost;username=guest;password=guest");

            // Action
            var host = con.GetValue("host");

            // Assert
            Assert.AreEqual("localhost", host);
        }
    }
}
// ReSharper restore InconsistentNaming