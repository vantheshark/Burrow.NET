using System;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.SharedEventBrokerTests
{
    [TestClass]
    public class BroadCastEvent
    {
        [TestMethod]
        public void Should_catch_all_exception()
        {
            // Arrange
            var watcher = Substitute.For<IRabbitWatcher>();
            var broker = new SharedEventBroker(watcher);
            broker.WhenAConsumerFinishedAMessage += (x, y) => { throw new Exception();};
            broker.WhenAConsumerGetAMessage += (x, y) => { throw new Exception(); };

            // Action
            broker.TellOthersAPriorityMessageIsHandled(null, 1);
            broker.TellOthersAPriorityMessageIsFinished(null, 1);

            //Assert
            watcher.Received(2).Error(Arg.Any<Exception>());
        }
    }
}
// ReSharper restore InconsistentNaming