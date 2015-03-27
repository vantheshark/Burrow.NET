using System;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.ConnectionStringTests
{
    [TestFixture]
    public class MethodGetValue
    {
        [Test, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_try_to_get_value_not_in_connectionstring()
        {
            // Arrange
            var con = new ConnectionString("host=localhost;username=guest;password=guest");

            // Action
            con.GetValue("port");
        }

        [Test]
        public void Should_return_value_provided_in_connectionstring()
        {
            // Arrange
            var con = new ConnectionString("host=localhost;username=guest;password=guest");

            
            // Assert
            Assert.AreEqual("localhost", con.Host);
            Assert.AreEqual("guest", con.UserName);
            Assert.AreEqual("guest", con.Password);
            Assert.AreEqual("/", con.VirtualHost);
            Assert.AreEqual(5672, con.Port);
        }

        [Test]
        public void Should_return_value_provided_in_connectionstring_using_caseinsensitive_key()
        {
            // Arrange
            var con = new ConnectionString("host=localhost;virtualHost=UAT;username=guest;password=guest");

            // Action
            var virtualHost = con.GetValue("virtualhost");

            // Assert
            Assert.AreEqual("UAT", virtualHost);
        }

        [Test]
        public void Should_return_proper_host_and_port_if_provided_in_the_host_section()
        {
            var con = new ConnectionString("host=localhost:5673;username=guest;password=guest");
            Assert.AreEqual(5673, con.Port);
            Assert.AreEqual("localhost", con.Host);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_provided_key_is_null()
        {
            var con = new ConnectionString("host=localhost:5673;username=guest;password=guest");
            con.GetValue(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void When_get_with_default_value_should_throw_exception_if_provided_key_is_null()
        {
            var con = new ConnectionString("host=localhost:5673;username=guest;password=guest");
            con.GetValue(null, "ABC");
        }
    }
}
// ReSharper restore InconsistentNaming