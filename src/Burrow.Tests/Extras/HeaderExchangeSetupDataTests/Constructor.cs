using Burrow.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.HeaderExchangeSetupDataTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod]
        public void Should_set_Durable_true_and_ExchangeType_Headers_by_default()
        {
            // Arrange & Action
            var data = new HeaderExchangeSetupData();

            // Asert
            Assert.IsTrue(data.Durable);
            Assert.AreEqual("headers", data.ExchangeType);
        }
    }
}
// ReSharper restore InconsistentNaming