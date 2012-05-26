using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Burrow.Tests.Internal.InteruptableSemaphoreTests
{
    [TestClass]
    public class MethodInterupt
    {
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void Should_return_0_if_already_interupted()
// ReSharper restore InconsistentNaming
        {
            // Arrange
            var sem = new InteruptableSemaphore(1, 1);

            // Acction
            var firstInterupt  = sem.Interupt();

            // Assert
            Assert.AreEqual(1, firstInterupt);
            Assert.AreEqual(0, sem.Interupt());
        }
    }
}
