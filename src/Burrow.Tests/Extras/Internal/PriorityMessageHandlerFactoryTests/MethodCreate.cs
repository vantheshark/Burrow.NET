using Burrow.Extras.Internal;
using NUnit.Framework;
using Burrow.Tests.Extras.RabbitSetupTests;


namespace Burrow.Tests.Extras.Internal.PriorityMessageHandlerFactoryTests
{
    [TestFixture]
    public class MethodCreate
    {
        [Test]
        public void Should_return_PriorityMessageHandler()
        {
            // Arrange
            var factory = new PriorityMessageHandlerFactory(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                            NSubstitute.Substitute.For<ISerializer>(),
                                                            NSubstitute.Substitute.For<IRabbitWatcher>());

            // Action
            var handler = factory.Create<Customer>("supscriptionName",  (x, y) => { });

            // Assert
            Assert.IsInstanceOfType(typeof(PriorityMessageHandler<Customer>), handler);
        }
    }
}
