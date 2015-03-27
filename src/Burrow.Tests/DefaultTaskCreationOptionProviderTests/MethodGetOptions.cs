using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultTaskCreationOptionProviderTests
{
    [TestFixture]
    public class MethodGetOptions
    {
        [Test]
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