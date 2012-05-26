using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.SubscriptionTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exception_if_use_null_model()
        {
            // Action
            new Subscription(null);
        }


        [TestMethod]
        public void Should_initialize_object_with_provided_IModel()
        {
            // Action
            new Subscription(NSubstitute.Substitute.For<IModel>());
        }
    }
}
// ReSharper restore InconsistentNaming