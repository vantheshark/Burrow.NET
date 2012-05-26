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
