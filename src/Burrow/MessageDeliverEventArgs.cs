
namespace Burrow
{
    public class MessageDeliverEventArgs
    {
        public string SubscriptionName { get; set; }
        public string ConsumerTag { get; set; }
        public ulong DeliveryTag { get; set; }
        public uint MessagePriority { get; set; }
    }
}
