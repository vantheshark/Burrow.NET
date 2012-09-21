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

            var client = RpcClientFactory.Create<ISomeService>();

            IRpcServerCoordinator server = new BurrowRpcServerCoordinator<ISomeService>(new DummyImplementation(), new BurrowRpcLoadBalancerRouteFinder());

            server.Start();

            string outValue;
            client.TryParse(out outValue);

            Global.DefaultWatcher.InfoFormat("Rpc call is dispatched, change the method to something else to test");
            Console.ReadKey();
        }

        public static void PrintHelp()
        {
            Global.DefaultWatcher.InfoFormat("This console app demonstrates how to use Burrow.RPC package!!!");
            Console.WriteLine();
            Global.DefaultWatcher.InfoFormat("   1/ First it'll create the queues for requests and responses");
            Global.DefaultWatcher.InfoFormat("   2/ Then it'll send the request to the request queue and wait for response if the method is decorated with Async attribute");
            Console.WriteLine();
            Global.DefaultWatcher.InfoFormat("Press anykey to run...");
            Console.WriteLine();
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
