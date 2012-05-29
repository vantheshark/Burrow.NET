using System.Threading;
using RabbitMQ.Client;

namespace Burrow.Extras.Internal
{
    internal class PriorityBurrowConsumer : BurrowConsumer
    {
        public SharedEventBroker EventBroker { get; private set; }
        public int QueuePriorirty { get; set; }
        private readonly Timer _timer;
        
        public PriorityBurrowConsumer(SharedEventBroker eventBroker, int priority, IModel channel, IMessageHandler messageHandler, IRabbitWatcher watcher, string consumerTag, bool autoAck, int batchSize) 
            : base(channel, messageHandler, watcher, consumerTag, autoAck, batchSize)
        {
            QueuePriorirty = priority;
            SetEventBroker(eventBroker);
            _timer = new Timer(x => Resume(priority - QueuePriorirty), null, Timeout.Infinite, Timeout.Infinite);
        }

        public void SetEventBroker(SharedEventBroker broker)
        {
            if (EventBroker != null)
            {
                EventBroker.WhenAConsumerGetAMessage -= WhenAConsumerGetAMessage;
                EventBroker.WhenAConsumerFinishedAMessage -= WhenAConsumerFinishedAMessage;
            }
            
            EventBroker = broker;

            if (EventBroker != null)
            {
                EventBroker.WhenAConsumerGetAMessage += WhenAConsumerGetAMessage;
                EventBroker.WhenAConsumerFinishedAMessage += WhenAConsumerFinishedAMessage;
            }
        }

        private void WhenAConsumerFinishedAMessage(IBasicConsumer consumer, int priority)
        {
            if (priority > QueuePriorirty)
            {
                _timer.Change(1000 * priority, Timeout.Infinite);
            }
        }

        private void WhenAConsumerGetAMessage(IBasicConsumer consumer, int priority)
        {
            if (priority > QueuePriorirty)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Interupt(priority - QueuePriorirty);
            }
        }

        internal virtual void Resume(int releaseCount)
        {
            if (_pool.Resume() > 0)
            {
                _watcher.InfoFormat("Consumer {0} [P {1}] resumed", ConsumerTag, QueuePriorirty);
            }
        }

        internal virtual void Interupt(int volumn)
        {
            if (_pool.Interupt() > 0)
            {
                _watcher.InfoFormat("Consumer {0} [P {1}] interupted", ConsumerTag, QueuePriorirty);
            }
        }

        public override void Dispose()
        {
            if (EventBroker != null)
            {
                EventBroker.WhenAConsumerGetAMessage -= WhenAConsumerGetAMessage;
                EventBroker.WhenAConsumerFinishedAMessage -= WhenAConsumerFinishedAMessage;
            }
            _timer.Dispose();
            base.Dispose();
        }
    }
}
