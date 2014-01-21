using System.Collections.Generic;
using Burrow.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;

namespace Burrow.Tests.Internal.BasicPropertiesWrapperTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod]
// ReSharper disable InconsistentNaming
        public void Should_wrap_a_IBasicProperties_instance()
// ReSharper restore InconsistentNaming
        {
            // Arrange
            var p = Substitute.For<IBasicProperties>();
            p.ContentType.Returns("ct");
            p.ContentEncoding.Returns("ce");
            p.DeliveryMode.Returns((byte)1);
            p.Priority.Returns((byte)10);
            p.CorrelationId.Returns("ci");
            p.ReplyTo.Returns("rt");
            p.Expiration.Returns("e");
            p.MessageId.Returns("mi");
            p.Timestamp.Returns(new AmqpTimestamp(1));
            p.Type.Returns("t");
            p.UserId.Returns("ui");
            p.AppId.Returns("ai");
            p.ClusterId.Returns("cli");
            p.IsHeadersPresent().Returns(true);
            p.Headers.Returns(new Dictionary<string, object>{{"Priority", 10}});
            

            // Action
            var w = new BasicPropertiesWrapper(p);

            // Assert
            Assert.AreEqual("ct", w.ContentType);
            Assert.AreEqual("ce", w.ContentEncoding);
            Assert.AreEqual((byte)1, w.DeliveryMode);
            Assert.AreEqual((byte)10, w.Priority);
            Assert.AreEqual("ci", w.CorrelationId);
            Assert.AreEqual("rt", w.ReplyTo);
            Assert.AreEqual("e", w.Expiration);
            Assert.AreEqual("mi", w.MessageId);
            Assert.AreEqual(p.Timestamp, new AmqpTimestamp(w.Timestamp));
            Assert.AreEqual("t", w.Type);
            Assert.AreEqual("ui", w.UserId);
            Assert.AreEqual("ai", w.AppId);
            Assert.AreEqual("cli", w.ClusterId);
            Assert.AreEqual(10, w.Headers["Priority"]);
        }
    }
}
