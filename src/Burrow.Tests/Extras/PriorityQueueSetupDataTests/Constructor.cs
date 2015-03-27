using Burrow.Extras;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.PriorityQueueSetupDataTests
{
    [TestFixture]
    public class Constructor
    {
        [Test]
        public void Should_initialize_obj_with_MaxPriorityLevel()
        {
            // Arrange & Action
            var data = new PriorityQueueSetupData(3);

            // Asert
            Assert.AreEqual((uint)3, data.MaxPriorityLevel);
            Assert.IsTrue(data.Durable);
        }
    }
}
// ReSharper restore InconsistentNaming