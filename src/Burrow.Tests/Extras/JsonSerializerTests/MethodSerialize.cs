using Burrow.Extras;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Burrow.Tests.Extras.JsonSerializerTests
{
    [TestClass]
    public class MethodSerialize
    {
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void Can_serialize_object()
// ReSharper restore InconsistentNaming
        {
            // Arrange
            var serializer = new JsonSerializer();


            // Action
            var str = serializer.Serialize(new {Name = "Bunny", Age = 30});
            dynamic obj = serializer.Deserialize<object>(str);

            // Asert
            Assert.AreEqual("Bunny", obj.Name.ToString());
            Assert.AreEqual("30", obj.Age.ToString());
        }
    }
}
