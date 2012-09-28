using System;
using Burrow.Extras;
using Burrow.RPC;

namespace Burrow.RpcTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Global.DefaultSerializer = new JsonSerializer();
            Global.DefaultWatcher.IsDebugEnable = false;

            PrintHelp();

            

            ISomeService realService = new DummyImplementation();
            IRpcServerCoordinator server = RpcFactory.CreateServer(realService, serverId: "UnitTest");
            server.Start();


            string outValue;
            var client = RpcFactory.CreateClient<ISomeService>();
            client.TryParse(out outValue);


            
            Console.WriteLine();
            Console.WriteLine();
            Global.DefaultWatcher.InfoFormat("Method TryParse was executed remotely, out value is {0}", outValue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            Global.DefaultWatcher.InfoFormat("... change the method to something else to continue to test");
            Console.ReadKey();
        }

        public static void PrintHelp()
        {
            Global.DefaultWatcher.InfoFormat("This console app demonstrates how to use Burrow.RPC package!!!");
            Console.WriteLine();
            Global.DefaultWatcher.InfoFormat("   1/ First, it'll create the queues for requests and responses");
            Global.DefaultWatcher.InfoFormat("   2/ Then it'll send the request to the request queue");
            Global.DefaultWatcher.InfoFormat("      and wait for response if the method is Sync");
            
            Console.WriteLine();
            Global.DefaultWatcher.InfoFormat("Press anykey to run...");
            Console.WriteLine();
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
