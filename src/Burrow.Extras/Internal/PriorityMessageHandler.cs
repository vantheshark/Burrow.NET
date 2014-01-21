using System;
using RabbitMQ.Client.Events;

namespace Burrow.Extras.Internal
{
    internal class PriorityMessageHandler
    {
        public static int GetMsgPriority(BasicDeliverEventArgs eventArg)
        {
            var priority = -1;
            if (eventArg.BasicProperties.Headers != null && eventArg.BasicProperties.Headers.ContainsKey("Priority"))
            {
                //It's a byte, has to convert to char
                var enc = new System.Text.UTF8Encoding();
                string str = enc.GetString((byte[])eventArg.BasicProperties.Headers["Priority"]);
                int.TryParse(str, out priority);
            }
            return priority;
        }
    }

    internal class PriorityMessageHandler<T> : DefaultMessageHandler<T>
    {
        public PriorityMessageHandler(string subscriptionName, Action<T, MessageDeliverEventArgs> msgHandlingAction, IConsumerErrorHandler consumerErrorHandler, ISerializer messageSerializer, IRabbitWatcher watcher) 
            : base(subscriptionName, msgHandlingAction, consumerErrorHandler, messageSerializer, watcher)
        {
        }

        protected override void HandleMessage(BasicDeliverEventArgs eventArgs, out bool msgHandled)
        {
            var priority = PriorityMessageHandler.GetMsgPriority(eventArgs);
            var currentThread = System.Threading.Thread.CurrentThread;
            currentThread.IsBackground = true;
#if DEBUG
            _watcher.DebugFormat("4. A task to execute the provided callback with DTag: {0} by CTag: {1}, Priority {2} has been started using {3}.",
                                 eventArgs.DeliveryTag,
                                 eventArgs.ConsumerTag,
                                 Math.Max(priority, 0),
                                 currentThread.IsThreadPoolThread ? "ThreadPool" : "dedicated Thread");
#endif
            CheckMessageType(eventArgs.BasicProperties);
            var message = _messageSerializer.Deserialize<T>(eventArgs.Body);
            _msgHandlingAction(message, new MessageDeliverEventArgs
            {
                ConsumerTag = eventArgs.ConsumerTag,
                DeliveryTag = eventArgs.DeliveryTag,
                SubscriptionName = _subscriptionName,
                MessagePriority = (uint)Math.Max(priority, 0)
            });
#if DEBUG
            _watcher.DebugFormat("5. A task to execute the provided callback with DTag: {0} by CTag: {1}, Priority {2} has been finished successfully.",
                                 eventArgs.DeliveryTag,
                                 eventArgs.ConsumerTag,
                                 Math.Max(priority, 0));
#endif
            msgHandled = true;
        }
    }
}