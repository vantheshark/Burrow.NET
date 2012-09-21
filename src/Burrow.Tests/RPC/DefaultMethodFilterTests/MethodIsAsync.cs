using System;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultMethodFilterTests
{
    [TestClass]
    public class MethodIsAsync
    {
        [TestMethod]
        public void Should_return_true_if_attributes_are_null()
        {
            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            var result = filter.IsAsync(null, null);

            // Assert
            Assert.IsFalse(result);

        }

        [TestMethod]
        public void Should_return_false_if_attributes_not_contain_AsyncAttribute()
        {
            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            var result = filter.IsAsync(null, new Attribute[0]);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Should_return_true_if_attributes_contain_AsyncAttribute()
        {
            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            var result = filter.IsAsync(null, new Attribute[] {new AsyncAttribute()});

            // Assert
            Assert.IsTrue(result);
        }
    }
}
// ReSharper restore InconsistentNaming