using System;
using System.Threading;
using Burrow.Extras;
using Burrow.Extras.Internal;
using Burrow.Publisher.Models;

namespace Burrow.Subscriber
{
    public static class TestSubscribingFromPriorityQueues
    {
        public static void Start()
        {
            Global.DefaultWatcher.InfoFormat("* The target queue is Burrow.Queue.BurrowTestApp.Bunny");
            Thread.Sleep(2000);
            PrintNotes();
            Console.ReadLine();
            const ushort maxPriorityLevel = 3;
            var tunnel = RabbitTunnel.Factory.WithPrioritySupport()
                                     .Create().WithPrioritySupport();
            
            tunnel.Subscribe(new PrioritySubscriptionOption<Bunny>
            {
                SubscriptionName = "BurrowTestApp",
                MaxPriorityLevel = maxPriorityLevel,
                MessageHandler = ProcessMessage,
                QueuePrefetchSize = 10,
                BatchSize = 2
            });
        }

        public static void StartAsync()
        {
            Global.DefaultWatcher.InfoFormat("* Target PRIORITY queues are Burrow.Queue.BurrowTestApp.Bunny_Priority0 -> 3");
            Thread.Sleep(2000);
            PrintNotes();
            Console.ReadLine();
            const ushort maxPriorityLevel = 3;
            
            var tunnel = RabbitTunnel.Factory.WithPrioritySupport()
                                     .Create().WithPrioritySupport();

            var totalMsg = tunnel.GetMessageCount(new PrioritySubscriptionOption<Bunny>
            {
                SubscriptionName = "BurrowTestApp",
                MaxPriorityLevel = maxPriorityLevel
            });
            Global.DefaultWatcher.InfoFormat(string.Format("There are total {0} messages in all priority queues", totalMsg));

            // SubscribeAsync auto Ack
            CompositeSubscription subscription = null;
            Action<Bunny, MessageDeliverEventArgs> messageHandler = (bunny, subscriptionData) =>
            {
                var error = false;
                try
                {
                    ProcessMessage(bunny);
                }
                catch (Exception)
                {
                    error = true;
                    if (subscription != null)
                    {
                        subscription.Nack(subscriptionData.ConsumerTag, subscriptionData.DeliveryTag, false);
                    }
                }
                finally
                {
                    if (subscription != null && !error)
                    {
                        subscription.Ack(subscriptionData.ConsumerTag, subscriptionData.DeliveryTag);
                    }
                }
            };

            subscription = tunnel.SubscribeAsync(new PriorityAsyncSubscriptionOption<Bunny>
            {
                SubscriptionName = "BurrowTestApp",
                MaxPriorityLevel = maxPriorityLevel,
                MessageHandler = messageHandler,
                QueuePrefetchSize = 10,
                BatchSize = 1
            });
        
        }

        private static void ProcessMessage(Bunny bunny)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            var processingTime = rand.Next(10, 100);
            if (processingTime % 5 == 0)
            {
                throw new Exception(
                    "This is a test exception to demonstrate how a message is handled once something wrong happens: " +
                    "Got a bad bunny, It should be put to Burrow.Queue.Error ;)");
            }
            Thread.Sleep(processingTime);
            Global.DefaultWatcher.InfoFormat("Processed msg [{0}], priority [{1}] in [{2}] ms\n", bunny.Name, bunny.Age, processingTime);
        }

        private static void PrintNotes()
        {
            Global.DefaultWatcher.InfoFormat("* You should run the publisher first to have some messages in the priority queues to see how the subscriber works!");
            Thread.Sleep(2000);
            Global.DefaultWatcher.InfoFormat("* You will see that messages from Priority3 queue will be processed first");
            Thread.Sleep(2000);
            Global.DefaultWatcher.InfoFormat("* By default, msgs from higher priority queue will be processed before messages from other lower priority queues.");
            Thread.Sleep(2000);
            Global.DefaultWatcher.InfoFormat("* Press anykey to start ... ");
        }
    }
}
