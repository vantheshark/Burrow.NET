using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.ConnectionStringTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_excepton_if_provide_null_connection_string()
        {
            new ConnectionString(null);
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_excepton_if_provide_invalid_connection_string()
        {
            new ConnectionString("a=b;c=d=e");
        }
    }
}
// ReSharper restore InconsistentNaming}
