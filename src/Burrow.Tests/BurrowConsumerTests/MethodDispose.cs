using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestClass]
    public class MethodDispose
    {
        [TestMethod]
        public void Can_called_many_times()
        {
            // Arrange
            var consumer = new BurrowConsumer(NSubstitute.Substitute.For<IModel>(), NSubstitute.Substitute.For<IMessageHandler>(),
                                              NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag", false, 3);

            // Action
            consumer.Dispose();
            consumer.Dispose();
        }
    }
}
// ReSharper restore InconsistentNaming