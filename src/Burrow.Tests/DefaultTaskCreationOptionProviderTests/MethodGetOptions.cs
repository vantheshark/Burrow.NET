using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultTaskCreationOptionProviderTests
{
    [TestClass]
    public class MethodGetOptions
    {
        [TestMethod]
        public void Should_return_long_running_if_ThreadPool_is_full()
        {
            // Arrange
            var t = new DefaultTaskCreationOptionProvider(1023);
            ThreadPool.SetMaxThreads(1023, 1000);

            // Action
            var options = t.GetOptions();

            // Assert
            Assert.AreEqual(TaskCreationOptions.LongRunning, options);

        }
    }
}
// ReSharper restore InconsistentNaming