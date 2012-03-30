using System;
using Burrow.Publisher.Models;

namespace Burrow.Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Click any key to subscribe to queue Burrow.Queue.BurrowTestApp.Bunny");
            Console.ReadLine();

            var tunnel = RabbitTunnel.Factory.Create();
            Global.DefaultPersistentMode = true;
            Global.DefaultConsumerBatchSize = 1000;

            // SubscribeAsync auto Ack
            tunnel.SubscribeAsync<Bunny>("BurrowTestApp", ProcessMessage);





            // SubscribeAsync manual Ack
            Subscription subscription = null;
            subscription = tunnel.SubscribeAsync<Bunny>("BurrowTestApp", (bunny, subscriptionData) =>
            {
                try
                {
                    ProcessMessage(bunny);
                }
                finally
                {
                    if (subscription != null)
                    {
                        subscription.Ack(subscriptionData.DeliveryTag);
                    }
                }
            });
            




            Console.ReadKey();
        }


        private static void ProcessMessage(Bunny bunny)
        {
            var rand = new Random((int) DateTime.Now.Ticks);
            var processingTime = rand.Next(1000, 1500);
            if (bunny.Age%5 == 0)
            {
                throw new Exception(
                    "This is a test exception to demonstrate how a message is handled once something wrong happens: " +
                    "Got a bad bunny, It should be put to Error Queue ;)");
            }
            System.Threading.Thread.Sleep(processingTime);
            Console.WriteLine("Got a bunny {0}, feeding it in {1} ms\n", bunny.Name, processingTime);
        }
    }
}
