using System;
using Burrow.Extras.Internal;
using NUnit.Framework;


namespace Burrow.Tests.Extras.Internal.CompositeSubscriptionTests
{
    [TestFixture]
    public class MethodAddSubscription
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
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
