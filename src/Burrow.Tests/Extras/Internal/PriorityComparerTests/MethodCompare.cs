using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityComparerTests
{
    [TestClass]
    public class MethodCompare
    {
        [TestMethod]
        public void Should_return_1_if_object_on_the_left_has_higher_priority_value()
        {
            // Arrange
            var comparer = new PriorityComparer<GenericPriorityMessage<int>>();
            var m1 = new GenericPriorityMessage<int>(1, 0);
            var m2 = new GenericPriorityMessage<int>(1, 10);

            // Action & Assert
            Assert.AreEqual(1, comparer.Compare(m2, m1));
        }

        [TestMethod]
        public void Should_return_minus_1_if_object_on_the_left_has_lower_priority_value()
        {
            // Arrange
            var comparer = new PriorityComparer<GenericPriorityMessage<int>>();
            var m1 = new GenericPriorityMessage<int>(1, 10);
            var m2 = new GenericPriorityMessage<int>(1, 0);

            // Action & Assert
            Assert.AreEqual(-1, comparer.Compare(m2, m1));
        }

        [TestMethod]
        public void Should_return_1_if_object_on_the_left_is_older_than_the_other_one()
        {
            // Arrange
            var comparer = new PriorityComparer<GenericPriorityMessage<int>>();
            var m1 = new GenericPriorityMessage<int>(1, 0);
            System.Threading.Thread.Sleep(1);
            var m2 = new GenericPriorityMessage<int>(1, 0);

            // Action & Assert
            Assert.AreEqual(1, comparer.Compare(m1, m2));
        }

        [TestMethod]
        public void Should_return_minus_1_if_object_on_the_left_is_younger_than_the_other_one()
        {
            // Arrange
            var comparer = new PriorityComparer<GenericPriorityMessage<int>>();
            var m1 = new GenericPriorityMessage<int>(1, 0);
            System.Threading.Thread.Sleep(1);
            var m2 = new GenericPriorityMessage<int>(1, 0);

            // Action & Assert
            Assert.AreEqual(-1, comparer.Compare(m2, m1));
        }

        [TestMethod]
        public void Should_return_0_for_other_case()
        {
            // Arrange
            var comparer = new PriorityComparer<GenericPriorityMessage<int>>();
            var m1 = new GenericPriorityMessage<int>(1, 0);
            var m2 = new GenericPriorityMessage<int>(1, 0);

            // Action & Assert
            Assert.AreEqual(0, comparer.Compare(m2, m1));
        }
    }
}
// ReSharper restore InconsistentNaming