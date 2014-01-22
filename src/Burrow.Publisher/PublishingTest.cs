using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Burrow.Extras;
using Burrow.Publisher.Models;

namespace Burrow.Publisher
{
    public static class PublishingTest
    {
        public static void Publish(int totalThread, int numberOfRabbitToCreatePerThread)
        {
            var tunnel = RabbitTunnel.Factory.Create();

            tunnel.SetSerializer(new JsonSerializer());

            var tasks = new List<Task>();
            var sw = new Stopwatch();
            sw.Start();
            tunnel.DedicatedPublishingChannel.ConfirmSelect();
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
            tunnel.DedicatedPublishingChannel.WaitForConfirmsOrDie();

            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            Console.WriteLine(string.Format("Published {0} \"rabbits\" in {1}.", numberOfRabbitToCreatePerThread * totalThread, sw.Elapsed));
            Console.ReadKey();
        }

        public static void PublishRandomPriorityMessages(int maxPriority)
        {
            var tunnel = RabbitTunnel.Factory.WithPrioritySupport().Create().WithPrioritySupport();

            tunnel.SetSerializer(new JsonSerializer());
            const int msgToPublish = 10000;

            uint index;
            for (index = 0; index < msgToPublish; index++)
            {
                try
                {
                    var priority = new Random(DateTime.Now.Millisecond).Next(maxPriority + 1);

                    tunnel.Publish(new Bunny
                    {
                        Age = (uint)priority,
                        Color = "White",
                        Name = "The Energizer Bunny"
                    }, (uint)priority);

                    //System.Threading.Thread.Sleep(30);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
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
