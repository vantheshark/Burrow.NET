using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Burrow.Tests.Internal.ConsoleWatcherTests
{
    [TestClass]
    public class AllWriteLogMethods
    {
        [TestMethod]
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
