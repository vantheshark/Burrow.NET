using System;
using RabbitMQ.Client;

namespace Burrow.Extras.Internal
{
    public delegate void ConsumerGetAMessage(IBasicConsumer consumer, int priority);
    public delegate void ConsumerFinishedAMessage(IBasicConsumer consumer, int priority);

    internal class SharedEventBroker
    {
        private readonly IRabbitWatcher _watcher;

        public SharedEventBroker(IRabbitWatcher watcher)
        {
            _watcher = watcher;
        }

        public event ConsumerGetAMessage WhenAConsumerGetAMessage;
        public event ConsumerFinishedAMessage WhenAConsumerFinishedAMessage;

        public void TellOthersAPriorityMessageIsHandled(IBasicConsumer sender, int msgPriority)
        {
            if (WhenAConsumerGetAMessage != null)
            {
                try
                {
                    WhenAConsumerGetAMessage(sender, msgPriority);
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }
                
            }
        }

        public void TellOthersAPriorityMessageIsFinished(IBasicConsumer sender, int msgPriority)
        {
            if (WhenAConsumerFinishedAMessage != null)
            {
                try
                {
                    WhenAConsumerFinishedAMessage(sender, msgPriority);
                }
                catch (Exception ex)
                {
                    _watcher.Error(ex);
                }
            }
        }
    }
}
