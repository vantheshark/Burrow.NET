using System;
using Burrow.Extras;
using Burrow.Extras.Internal;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.TunnelFactoryExtensionsTests
{
    [TestFixture]
    public class MethodChangePriorityQueueSuffixConvention
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_setting_null_value()
        {
            // Arrange
            ITunnelWithPrioritySupport tunnel = null;

            // Action
            tunnel.ChangePriorityQueueSuffixConvention(null);
        }

        [Test]
        public void Should_change_the_suffix_to_new_value()
        {
            // Arrange
            var newSuffixConvention = Substitute.For<IPriorityQueueSuffix>();
            ITunnelWithPrioritySupport tunnel = null;

            // Action
            tunnel.ChangePriorityQueueSuffixConvention(newSuffixConvention);

            // Assert
            Assert.AreSame(newSuffixConvention, PriorityQueuesRabbitSetup.GlobalPriorityQueueSuffix);
            tunnel.ChangePriorityQueueSuffixConvention(new PriorityQueueSuffix());
        }
    }


}
// ReSharper restore InconsistentNaming