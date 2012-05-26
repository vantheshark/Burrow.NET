using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Burrow.Tests.Internal.InteruptableSemaphoreTests
{
    [TestClass]
    public class MethodResume
    {
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void Should_return_0_if_already_resumed()
// ReSharper restore InconsistentNaming
        {
            // Arrange
            var sem = new InteruptableSemaphore(1, 1);
            sem.Interupt();

            // Acction
            var firstResume  = sem.Resume();

            // Assert
            Assert.AreEqual(1, firstResume);
            Assert.AreEqual(0, sem.Resume());
        }
    }
}
