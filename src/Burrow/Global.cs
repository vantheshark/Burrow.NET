using Burrow.Internal;

namespace Burrow
{
    public static class Global
    {
        public static ISerializer DefaultSerializer = new JavaScriptSerializer();
        public static IRabbitWatcher DefaultWatcher = new ConsoleWatcher();
        public static ICorrelationIdGenerator DefaultCorrelationIdGenerator = new SimpleCorrelationIdGenerator();
        public static ITypeNameSerializer DefaultTypeNameSerializer = new TypeNameSerializer();
        
        /// <summary>
        /// The higher the number is, the more threads a tunnel will create to consume messages in the queue.
        /// If set to 1, it means the messages will be consumed sequently
        /// This value is used by the TunnelFactory when it create a RabbitTunnel
        /// This value is also used to call IModel.BasicQos, which eventually sets the maximum amount of messages stay on the Unacknowledged list when they are consumed
        /// </summary>
        public static ushort DefaultConsumerBatchSize = 256;
        
        /// <summary>
        /// Set to true will save the message to disk when it's published, defalt is false.
        /// That's mean the messages will be removed when the server is restarted.
        /// This value is used by the TunnelFactory when it create a RabbitTunnel
        /// </summary>
        public static bool SetDefaultPersistentMode;
    }
}
