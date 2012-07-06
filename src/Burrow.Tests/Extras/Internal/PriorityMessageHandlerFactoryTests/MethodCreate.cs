using Burrow.Extras.Internal;
using Burrow.Tests.Extras.RabbitSetupTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Burrow.Tests.Extras.Internal.PriorityMessageHandlerFactoryTests
{
    [TestClass]
    public class MethodCreate
    {
        [TestMethod]
        public void Should_return_PriorityMessageHandler()
        {
            // Arrange
            var factory = new PriorityMessageHandlerFactory(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                            NSubstitute.Substitute.For<ISerializer>(),
                                                            NSubstitute.Substitute.For<IRabbitWatcher>());

            // Action
            var handler = factory.Create<Customer>("supscriptionName",  (x, y) => { });

            // Assert
            Assert.IsInstanceOfType(handler, typeof(PriorityMessageHandler<Customer>));
        }
    }
}
