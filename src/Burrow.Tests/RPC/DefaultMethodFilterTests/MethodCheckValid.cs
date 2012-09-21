using System;
using System.Reflection;
using System.Reflection.Emit;
using Burrow.RPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.RPC.DefaultMethodFilterTests
{
    [TestClass]
    public class MethodCheckValid
    {
        [TestMethod]
        public void Should_do_nothing_if_method_is_not_async()
        {
            // Arrange
            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            filter.CheckValid(new DynamicMethod("", typeof(int), null, true), null, false);
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_method_is_async_and_has_return_type()
        {
            MethodInfo info = new DynamicMethod("", typeof(int), null, true);

            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            filter.CheckValid(info, null, true);
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Should_throw_exception_if_method_is_async_and_has_out_param()
        {
            DefaultMethodFilter.CheckedMethodCaches.Clear();
            var type = typeof (ISomeService);
            var method = type.GetMethod("TryParse");

            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            filter.CheckValid(method, null, true);
        }

        [TestMethod]
        public void Should_not_throw_exception_if_method_is_async_and_has_ref_param()
        {
            var type = typeof(ISomeService);
            var method = type.GetMethod("Save");

            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            filter.CheckValid(method, null, true);
        }

        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void Should_throw_exception_if_method_has_delegate_param()
        {
            var type = typeof(ISomeService);
            
            var method = type.GetMethod("add_Event");

            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            filter.CheckValid(method, null, true);
        }

        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void Should_throw_exception_if_method_is_property_accessor()
        {
            var type = typeof(ISomeService);

            var method = type.GetMethod("get_Message");

            // Arrange
            var filter = new DefaultMethodFilter();

            // Action
            filter.CheckValid(method, null, true);
        }
    }
}
// ReSharper restore InconsistentNaming