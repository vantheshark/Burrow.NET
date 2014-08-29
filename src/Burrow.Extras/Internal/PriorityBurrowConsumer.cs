using System;
using Burrow.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Burrow.Extras.Internal
{
    internal class PriorityBurrowConsumer : BurrowConsumer
    {
        private static readonly object SyncRoot = new object();


        private CompositeSubscription _subscription;
        internal IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>> PriorityQueue;
        private uint _queuePriorirty;
        private string _sharedSemaphore;

        public PriorityBurrowConsumer(IModel channel, IMessageHandler messageHandler, IRabbitWatcher watcher, bool autoAck, int batchSize)
            : base(channel, messageHandler, watcher, autoAck, batchSize, false)
        {
        }

        ///<summary>Overrides DefaultBasicConsumer's OnCancel
        ///implementation, extending it to call the Close() method of
        ///the PriorityQueue.</summary>
        public override void OnCancel()
        {
            CloseQueue();
            base.OnCancel();
        }

        ///<summary>Overrides DefaultBasicConsumer's
        ///HandleBasicDeliver implementation, building a
        ///BasicDeliverEventArgs instance and placing it in the
        ///Priority Queue.</summary>
        public override void HandleBasicDeliver(string consumerTag,
                                                ulong deliveryTag,
                                                bool redelivered,
                                                string exchange,
                                                string routingKey,
                                                IBasicProperties properties,
                                                byte[] body)
        {
            //NOTE: This method is blocked by the RabbitMQ.Client if the unacked messages reach the prefetch size
            var e = new BasicDeliverEventArgs
            {
                ConsumerTag = consumerTag,
                DeliveryTag = deliveryTag,
                Redelivered = redelivered,
                Exchange = exchange,
                RoutingKey = routingKey,
                BasicProperties = properties,
                Body = body
            };
            PriorityQueue.Enqueue(new GenericPriorityMessage<BasicDeliverEventArgs>(e, _queuePriorirty));
        }

        protected override BasicDeliverEventArgs Dequeue()
        {
            if (PriorityQueue == null)
            {
                return null;
            }
            var msg = PriorityQueue.Dequeue();
                            
            if (msg != null && msg.Message != null)
            {
                return msg.Message;
            }
            return null;
        }

        protected override void CloseQueue()
        {
            if (PriorityQueue != null)
            {
                PriorityQueue.Close();
            }
        }


        public void Init(IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>> priorityQueue, CompositeSubscription subscription, uint priority, string sharedSemaphore)
        {
            if (priorityQueue == null)
            {
                throw new ArgumentNullException("priorityQueue");
            }
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }
            if (string.IsNullOrEmpty(sharedSemaphore))
            {
                throw new ArgumentNullException("sharedSemaphore");
            }

            _queuePriorirty = priority;
            _subscription = subscription;
            _sharedSemaphore = sharedSemaphore;
            PriorityQueue = priorityQueue;
            PriorityQueue.DeleteAll(msg => msg.Priority == priority);
        }

        public void Ready()
        {
            if (_subscription == null)
            {
                throw new Exception("Subscription not initialized, call Init first");
            }
            if (PriorityQueue == null)
            {
                throw new Exception("PriorityQueue not initialized, call Init first");
            }

            lock (SyncRoot)
            {
                _pool = new SafeSemaphore(_watcher, BatchSize, BatchSize, _sharedSemaphore);
            }

            StartConsumerThread(string.Format("Consumer thread: {0}, Priority queue: {1}", ConsumerTag, _queuePriorirty));
        }

        internal protected override void DoAck(BasicDeliverEventArgs basicDeliverEventArgs, IBasicConsumer subscriptionInfo)
        {
            if (_disposed)
            {
                return;
            }

            _subscription.Ack(basicDeliverEventArgs.ConsumerTag, basicDeliverEventArgs.DeliveryTag);
        }
   }
}
