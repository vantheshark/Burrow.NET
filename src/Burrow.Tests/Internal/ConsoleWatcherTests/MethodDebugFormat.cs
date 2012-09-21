using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsoleWatcherTests
{
    [TestClass]
    public class MethodDebugFormat
    {
        [TestMethod]
        public void Should_not_write_if_debug_is_disable()
        {
            // Arrange
            var watcher = new ConsoleWatcher();

            // Action
            watcher.DebugFormat("{Debug}", 1, 2);
        }

        [TestMethod]
        public void Should_write_if_debug_is_enable()
        {
            // Arrange
            var watcher = new ConsoleWatcher();
            watcher.IsDebugEnable = true;

            // Action
            watcher.DebugFormat("{Debug}", 1, 2);
        }
    }
}
// ReSharper restore InconsistentNaming