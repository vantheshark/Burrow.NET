using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.RabbitTunnelWithPriorityQueuesSupportTests
{
    [TestFixture]
    public class Constructor
    {
        [Test]
        public void Can_construct_with_provided_route_finder_and_durable_connection()
        {
            // Arrange
            var routeFinder = Substitute.For<IRouteFinder>();
            var durableConnection = Substitute.For<IDurableConnection>();
            ////durableConnection.ConnectionFactory.Returns(Substitute.For<ConnectionFactory>());

            // Action
            var tunnel = new RabbitTunnelWithPriorityQueuesSupport(routeFinder, durableConnection);

            // Assert
            Assert.IsNotNull(tunnel);
        }
    }
}
// ReSharper restore InconsistentNaming