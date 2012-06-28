using System;
using Burrow.Extras;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.TunnelFactoryExtensionsTests
{
    [TestClass]
    public class MethodChangePriorityQueueSuffixConvention
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_setting_null_value()
        {
            // Arrange
            ITunnelWithPrioritySupport tunnel = null;

            // Action
            tunnel.ChangePriorityQueueSuffixConvention(null);
        }

        [TestMethod]
        public void Should_change_the_suffix_to_new_value()
        {
            // Arrange
            var newSuffixConvention = NSubstitute.Substitute.For<IPriorityQueueSuffix>();
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