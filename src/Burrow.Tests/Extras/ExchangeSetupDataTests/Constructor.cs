using Burrow.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.ExchangeSetupDataTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod]
        public void Should_set_Durable_true_and_ExchangeType_Direct_by_default()
        {
            // Arrange & Action
            var data = new ExchangeSetupData();

            // Asert
            Assert.IsTrue(data.Durable);
            Assert.AreEqual("direct", data.ExchangeType);
        }
    }
}
// ReSharper restore InconsistentNaming