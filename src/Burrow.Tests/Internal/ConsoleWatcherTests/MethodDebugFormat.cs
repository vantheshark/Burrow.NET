using Burrow.Internal;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.ConsoleWatcherTests
{
    [TestFixture]
    public class MethodDebugFormat
    {
        [Test]
        public void Should_not_write_if_debug_is_disable()
        {
            // Arrange
            var watcher = new ConsoleWatcher();

            // Action
            watcher.DebugFormat("{Debug}", 1, 2);
        }

        [Test]
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