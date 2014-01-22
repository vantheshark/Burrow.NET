using System;
using Burrow.Extras;

namespace Burrow.Publisher
{
    class Program
    {
        /// <summary>
        /// Run this test if you have RabbitMQ installed on localhost, otherwise change the connection string in app.config
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Global.DefaultSerializer = new JsonSerializer();
            Global.DefaultWatcher.InfoFormat("This demo will show you how Burrow.NET publish messages from RabbitMQ.\nPress anykey to continue!!!");
            Console.ReadKey();
            Console.Clear();
            TestSetupNormalQueue();
            
            Console.Clear();
            TestSetupPriorityQueues();

            /*
             NOTE:
             There are 2 methods on  RabbitSetupTest
             * CreateExchangesAndQueues(string exchangeName, string queueName, string routingKey)
             and
             * DestroyExchangesAndQueues(string exchangeName, string queueName)
             that demonstrate how to create/destroy the exchange/queue by their known names without a need of implementing RouteFinder
             */

            RabbitTunnel.Factory.CloseAllConnections();
            Console.Clear();
            Global.DefaultWatcher.InfoFormat("Demo finished. Press anykey to quit!");
            Console.ReadKey();
            Environment.Exit(0);
        }

        private static void TestSetupNormalQueue()
        {
            RabbitSetupTest.CreateExchangesAndQueues();
            Global.DefaultWatcher.InfoFormat("Press anykey to publish messages ..."); 
            Console.ReadKey(false);
            PublishingTest.Publish(2, 5000);
            Global.DefaultWatcher.InfoFormat("If you check queue Burrow.Queue.BurrowTestApp.Bunny, you should see 10K msgs there");
            Console.WriteLine();
            Global.DefaultWatcher.InfoFormat("Now press anykey to destroy the queue and exchange");
            Console.ReadKey(false);
            RabbitSetupTest.DestroyExchangesAndQueues();
            Global.DefaultWatcher.InfoFormat("Queue Burrow.Queue.BurrowTestApp.Bunny and exchange Burrow.Exchange should be deleted now");
            Console.ReadKey(false);
        }


        private static void TestSetupPriorityQueues()
        {
            PriorityRabbitSetupTest.CreateExchangesAndQueues();
            Global.DefaultWatcher.InfoFormat("Press anykey to publish random priority messages to those queues ...");
            Console.ReadKey(false);
            PublishingTest.PublishRandomPriorityMessages(3);
            Global.DefaultWatcher.InfoFormat("If you check queue Burrow.Queue.BurrowTestApp.Bunny_Priority0 -> 4, you should see approximately 2500 msgs on each");
            Console.WriteLine();
            Global.DefaultWatcher.InfoFormat("Now press anykey to destroy all the queues and the exchange");
            Console.ReadKey(false);
            PriorityRabbitSetupTest.DestroyExchangesAndQueues();
            Global.DefaultWatcher.InfoFormat("All priority queues Burrow.Queue.BurrowTestApp.Bunny_Priority0 -> 4 and exchange Burrow.Exchange should be deleted now");
            Console.ReadKey(false);
        }
    }
}
