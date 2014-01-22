using System;
using Burrow.Extras;

namespace Burrow.Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            Global.DefaultSerializer = new JsonSerializer();
            Global.DefaultWatcher.InfoFormat("This demo will show you how Burrow.NET subscribe messages from RabbitMQ.\nPress anykey to continue!!!");
            Console.ReadKey(false);

            #region -- Run this test to start subscribe from normal queue --
            // This test also demonstrate the "DeadLetter" feature. Make sure queue "Burrow.Queue.Error" has already existed as DeadLetter msg will go to that queue
            // If queue "Burrow.Queue.Error" does not exist, try to create it from http://localhost:55672/#/queues
            // Or refer to Burrow.Publisher project to see how to create queue
            
            //TestSubscribing.Start();
            //TestSubscribing.StartAsync(); 
            #endregion

            #region -- Run this test to start subscribe from PRIORITY queues --
            TestSubscribingFromPriorityQueues.Start();
            //TestSubscribingFromPriorityQueues.StartAsync();
            #endregion


            Console.ReadKey(false);
            RabbitTunnel.Factory.CloseAllConnections();
            Global.DefaultWatcher.InfoFormat("Demo finished. Press anykey to quit!");
            Console.ReadKey(false);
            Environment.Exit(0);
        }
    }
}
