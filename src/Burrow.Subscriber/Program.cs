using System;
using Burrow.Extras;

namespace Burrow.Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            Global.DefaultSerializer = new JsonSerializer();
            PrintHelp();

            #region -- Run this test to start subscribe from normal queue --
            // This test also demonstrate the "DeadLetter" feature. Make sure queue "Burrow.Queue.Error" has already existed as DeadLetter msg will go to that queue
            // If queue "Burrow.Queue.Error" does not exist, try to create it from http://localhost:55672/#/queues
            TestSubscribing.Start(); 
            #endregion

            #region -- Run this test to start subscribe from PRIORITY queues --
            //TestSubscribingFromPriorityQueues.Start();
            #endregion

            #region -- Run this test to start subscribe from PRIORITY queues async --
            //TestSubscribingFromPriorityQueues.StartAsync();
            #endregion


            Console.ReadKey();
        }

        public static void PrintHelp()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Uncomment the test you want to run. Press anykey to continue!!!");
            Console.ReadKey();
            Console.ResetColor();
        }
    }
}
