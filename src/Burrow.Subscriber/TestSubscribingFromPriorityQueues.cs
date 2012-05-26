using System;
using Burrow.Extras;
using Burrow.Publisher.Models;

namespace Burrow.Subscriber
{
    public static class TestSubscribingFromPriorityQueues
    {
        public static void StartAsync()
        {
            Console.WriteLine("Click any key to subscribe to PRIORITY queue Burrow.Queue.BurrowTestApp.Bunny");
            Console.ReadLine();
            const ushort maxPriorityLevel = 3;
            Global.DefaultConsumerBatchSize = Math.Max((ushort)1, maxPriorityLevel);
            var tunnel = RabbitTunnel.Factory.WithPrioritySupport()
                                     .Create().WithPrioritySupport();

            // SubscribeAsync auto Ack
            tunnel.SubscribeAsync<Bunny>("BurrowTestApp", maxPriorityLevel, ProcessMessage);
        }

        public static void Start()
        {
            Console.WriteLine("Click any key to subscribe to PRIORITY queue Burrow.Queue.BurrowTestApp.Bunny");
            Console.ReadLine();
            const ushort maxPriorityLevel = 3;
            Global.DefaultConsumerBatchSize = Math.Max((ushort)1, maxPriorityLevel);
            var tunnel = RabbitTunnel.Factory.WithPrioritySupport()
                                     .Create().WithPrioritySupport();

            // SubscribeAsync auto Ack
            tunnel.Subscribe<Bunny>("BurrowTestApp", maxPriorityLevel, ProcessMessage);
        }

        private static void ProcessMessage(Bunny bunny)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            var processingTime = rand.Next(1000, 2000);
            System.Threading.Thread.Sleep(processingTime);
            Console.WriteLine("Processed msg [{0}], priority [{1}] in [{2}] ms\n", bunny.Name, bunny.Age, processingTime);
        }
    }
}
