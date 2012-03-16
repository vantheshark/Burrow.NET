using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Burrow.Extras;
using Burrow.Publisher.Models;

namespace Burrow.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            TestPublish(1, 50000 / 1);
        }

        private static void TestPublish(int totalThread, int numberOfRabbitToCreatePerThread)
        {
            var tunnel = RabbitTunnel.Factory.Create();
            tunnel.SetSerializer(new JsonSerializer());

            var tasks = new List<Task>();
            var sw = new Stopwatch();
            sw.Start();
            
            for (var i = 0; i < totalThread; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    uint index;
                    for (index = 0; index < numberOfRabbitToCreatePerThread; index++)
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
                }));
            }



            tasks.ForEach(x => x.Wait());
            sw.Stop();
            Console.WriteLine(string.Format("Published {0} \"rabbits\" in {1}.", numberOfRabbitToCreatePerThread * totalThread, sw.Elapsed));
            Console.ReadKey();
        }
    }
}
