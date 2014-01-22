using System;
using Burrow.Publisher.Models;

namespace Burrow.Subscriber
{
    public static class TestSubscribing
    {
        public static void Start()
        {
            Global.DefaultWatcher.InfoFormat("Click any key to subscribe to queue Burrow.Queue.BurrowTestApp.Bunny");
            Console.ReadLine();
            var tunnel = RabbitTunnel.Factory.Create();

            tunnel.Subscribe(new SubscriptionOption<Bunny>
            {
                BatchSize = 1,
                MessageHandler = ProcessMessage,
                QueuePrefetchSize = 10,
                SubscriptionName = "BurrowTestApp"
            });
            
        }

        public static void StartAsync()
        {
            Global.DefaultWatcher.InfoFormat("Click any key to asynchronously subscribe to queue Burrow.Queue.BurrowTestApp.Bunny");
            Console.ReadLine();
            var tunnel = RabbitTunnel.Factory.Create();

            Subscription subscription = null;
            subscription = tunnel.SubscribeAsync(new AsyncSubscriptionOption<Bunny>
            {
                BatchSize = 1,
                QueuePrefetchSize = 10,
                SubscriptionName = "BurrowTestApp",

                MessageHandler = (bunny, subscriptionData) =>
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
                            subscription.Nack(subscriptionData.DeliveryTag, false); 
                        }
                    }
                    finally
                    {
                        if (subscription != null && !error)
                        {
                            subscription.Ack(subscriptionData.DeliveryTag);
                        }
                    }
                }
            });
        }


        public static void ProcessMessage(Bunny bunny)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            var processingTime = rand.Next(1000, 1500);
            if (processingTime % 3 == 0)
            {
                throw new Exception(
                    "This is a test exception to demonstrate how a message is handled once something wrong happens: " +
                    "Got a bad bunny, It should be put to Burrow.Queue.Error ;)");
            }
            System.Threading.Thread.Sleep(processingTime);
            Global.DefaultWatcher.InfoFormat("Processed msg [{0}], priority [{1}] in [{2}] ms\n", bunny.Name, bunny.Age, processingTime);
        }
    }
}
