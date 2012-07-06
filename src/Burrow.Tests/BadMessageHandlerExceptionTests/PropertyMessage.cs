using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BadMessageHandlerExceptionTests
{
    [TestClass]
    public class PropertyMessage
    {
        [TestMethod]
        public void Should_return_a_predefined_string()
        {
            // Arrange
            var e = new BadMessageHandlerException(new Exception("Some exceptoin"));

            // Assert
            Assert.AreEqual("Method HandleMessage of the IMessageHandler should never throw any exception. If it's the built-in MessageHandler please contact the author asap!", e.Message);

        }
    }
}
// ReSharper restore InconsistentNaming