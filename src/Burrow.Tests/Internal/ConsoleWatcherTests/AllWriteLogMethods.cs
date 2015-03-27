using Burrow.Internal;
using NUnit.Framework;

namespace Burrow.Tests.Internal.ConsoleWatcherTests
{
    [TestFixture]
    public class AllWriteLogMethods
    {
        [Test]
// ReSharper disable InconsistentNaming
        public void Should_catch_all_potential_exceptions()
// ReSharper restore InconsistentNaming
        {
            // Arrange
            var watcher = new ConsoleWatcher();

            // Action
            watcher.WarnFormat("{0}{1}{2}", 1, 2);
            watcher.ErrorFormat("{0}{1}{2}", 1, 2);
        }
    }
}
