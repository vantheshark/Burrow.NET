using System;
using System.Collections;
using RabbitMQ.Client;

namespace Burrow.Internal
{
    [Serializable]
    public class BurrowError
    {
        public string RoutingKey { get; set; }
        public string Exchange { get; set; }
        public string Exception { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public BasicPropertiesWrapper BasicProperties { get; set; }
    }
    
    [Serializable]
    public class BasicPropertiesWrapper
    {
        public BasicPropertiesWrapper(IBasicProperties basicProperties)
        {
            ContentType = basicProperties.ContentType;
            ContentEncoding = basicProperties.ContentEncoding;
            DeliveryMode = basicProperties.DeliveryMode;
            Priority = basicProperties.Priority;
            CorrelationId = basicProperties.CorrelationId;
            ReplyTo = basicProperties.ReplyTo;
            Expiration = basicProperties.Expiration;
            MessageId = basicProperties.MessageId;
            Timestamp = basicProperties.Timestamp.UnixTime;
            Type = basicProperties.Type;
            UserId = basicProperties.UserId;
            AppId = basicProperties.AppId;
            ClusterId = basicProperties.ClusterId;

            if (basicProperties.IsHeadersPresent())
            {
                foreach (DictionaryEntry header in basicProperties.Headers)
                {
                    Headers.Add(header.Key, header.Value);
                }
            }
        }
        public string ContentType { get; set; }
        public string ContentEncoding { get; set; }
        public IDictionary Headers { get; set; }
        public byte DeliveryMode { get; set; }
        public byte Priority { get; set; }
        public string CorrelationId { get; set; }
        public string ReplyTo { get; set; }
        public string Expiration { get; set; }
        public string MessageId { get; set; }
        public long Timestamp { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
        public string AppId { get; set; }
        public string ClusterId { get; set; }
        public int ProtocolClassId { get; set; }
        public string ProtocolClassName { get; set; }
    }
}
