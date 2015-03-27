using System;
using NUnit.Framework;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestFixture]
    public class Constructor
    {
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_use_null_model()
        {
            // Action
            new Subscription(null);
        }


        [Test]
        public void Should_initialize_object_with_provided_IModel()
        {
            // Action
            new Subscription(NSubstitute.Substitute.For<IModel>());
        }
    }
}
// ReSharper restore InconsistentNaming