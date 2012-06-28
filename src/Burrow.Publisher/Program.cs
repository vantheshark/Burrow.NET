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
            PublishingTest.PrintHelp();

            #region -- Run this test to create and destroy exchange, queues programatically --
            //RabbitSetupTest.CreateExchangesAndQueues();
            //Console.Write("Press anykey to destroy them!");
            //Console.ReadKey();
            //RabbitSetupTest.DestroyExchangesAndQueues();


            //RabbitSetupTest.CreateExchangesAndQueues("EEEEE", "QQQQQ", "RRRRR");
            //Console.Write("Press anykey to destroy them!");
            //Console.ReadKey();
            //RabbitSetupTest.DestroyExchangesAndQueues("EEEEE", "QQQQQ");
            //RabbitSetupTest.DestroyExchangesAndQueues("Burrow.Exchange.Error", "Burrow.Queue.Error");
            #endregion

            #region -- Run this test to create and destroy PRIORITY exchange, queues programatically --
            PriorityRabbitSetupTest.CreateExchangesAndQueues();
            //Console.Write("Press anykey to destroy them!");
            //Console.ReadKey();
            //PriorityRabbitSetupTest.DestroyExchangesAndQueues();
            #endregion
            
            
            #region -- Run this test to publish normal messages to queue --
            //Console.WriteLine("Press anykey to publish messages ..."); Console.ReadKey();
            //PublishingTest.Publish(2, 1000);
            #endregion

            #region -- Run this test to publish PRIORITY messages to queue --
            //Console.WriteLine("Press anykey to publish messages ..."); Console.ReadKey();
            PublishingTest.PublishRandomPriorityMessages(3);
            #endregion


            Console.WriteLine("Finished. Press anykey to quit!");
            Console.ReadKey();
        }
    }
}
