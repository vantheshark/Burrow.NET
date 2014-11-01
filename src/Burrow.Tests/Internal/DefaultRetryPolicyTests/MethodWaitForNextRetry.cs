using System.Threading;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Internal.DefaultRetryPolicyTests
{
    [TestClass]
    public class MethodWaitForNextRetry
    {
        [TestMethod]
        public void Should_wait_and_execute_retry_action()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var policy = new DefaultRetryPolicy();

            // Action
            policy.WaitForNextRetry(() => are.Set());
            
            Assert.IsTrue(are.WaitOne(1000));

            // Assert
            Assert.AreEqual(1000, policy.DelayTime);
            Assert.IsFalse(policy.IsWaiting);
        }

        [TestMethod]
        public void Should_double_delay_time_for_next_retry()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var policy = new DefaultRetryPolicy();

            // Action
            policy.WaitForNextRetry(() => are.Set());
            Assert.IsTrue(are.WaitOne(3000));
            policy.WaitForNextRetry(() => are.Set());
            Assert.IsTrue(are.WaitOne(6000));
            policy.WaitForNextRetry(() => are.Set());
            Assert.IsTrue(are.WaitOne(12000));

            // Assert
            Assert.AreEqual(4000, policy.DelayTime);
            Assert.IsFalse(policy.IsWaiting);
        }

        [TestMethod]
        public void Should_double_delay_time_until_max_delay_value_is_reached()
        {
            // Arrange
            var are = new AutoResetEvent(false);
            var policy = new DefaultRetryPolicy(1000 * 2 /* 2 seconds */);

            // Action
            policy.WaitForNextRetry(() => are.Set());
            Assert.IsTrue(are.WaitOne(3000));
            policy.WaitForNextRetry(() => are.Set());
            Assert.IsTrue(are.WaitOne(6000));
            policy.WaitForNextRetry(() => are.Set());
            Assert.IsTrue(are.WaitOne(12000));

            // Assert
            Assert.AreEqual(2000, policy.DelayTime);
            Assert.IsFalse(policy.IsWaiting);
        }
    }
}
// ReSharper restore InconsistentNaming