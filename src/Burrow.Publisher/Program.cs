using System;
using System.Diagnostics;
using Burrow.Publisher.Models;

namespace Burrow.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            TestPublish(10000);
        }

        private static void TestPublish(int numberOfRabbitToCreate)
        {
            var tunnel = TunnelFactory.Create();
            var sw = new Stopwatch();
            sw.Start();
            uint index;
            for (index = 0; index < numberOfRabbitToCreate; index++)
            {
                try
                {
                    tunnel.Publish(new Bunny
                    {
                        Age = index,
                        Color = "White",
                        Name = "The Energizer Bunny"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
            sw.Stop();
            Console.WriteLine(string.Format("Publish {0} rabbits in {1}.", index, sw.Elapsed));
        }
    }
}
