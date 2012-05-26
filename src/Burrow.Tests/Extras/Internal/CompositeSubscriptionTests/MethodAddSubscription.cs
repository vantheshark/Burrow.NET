using System;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestClass]
    public class MethodAddSubscription
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void Should_throw_exception_if_privided_null_object()
// ReSharper restore InconsistentNaming
        {
            // Arrange
            var subs = new CompositeSubscription();

            // Action
            subs.AddSubscription(null);
            // Assert
        }
    }
}
