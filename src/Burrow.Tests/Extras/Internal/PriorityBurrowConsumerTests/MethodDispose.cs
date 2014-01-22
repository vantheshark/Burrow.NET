using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_8;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestClass]
    public class MethodDispose
    {
        [TestMethod]
        public void Can_called_many_times()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            queue.When(x => x.Dequeue()).Do(callInfo => waitHandler.WaitOne());
            var consumer = new PriorityBurrowConsumer(Substitute.For<IModel>(), Substitute.For<IMessageHandler>(), Substitute.For<IRabbitWatcher>(), false, 1);
            consumer.Init(queue, new CompositeSubscription(), 1, "sem");
            consumer.Ready();

            // Action
            consumer.Dispose();
            consumer.Dispose();
            waitHandler.Set();
        }


        [TestMethod]
        public void Can_wait_until_messageInProcess_down_to_0()
        {
            // Arrange
            var waitHandler = new AutoResetEvent(false);
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();


            var watcher = Substitute.For<IRabbitWatcher>();
            watcher.IsDebugEnable.Returns(true);
            //To decrease the messagages in progress so it doesn't have to wait when dispose at the end
            watcher.When(x => x.InfoFormat(Arg.Any<string>(), Arg.Any<object[]>()))
                   .Do(callInfo =>
            {
                //When print log for waiting, fire HandlingComplete on msgHandler to decrease the messageInProcessCount
                msgHandler.HandlingComplete += Raise.Event<MessageHandlingEvent>(new BasicDeliverEventArgs
                {
                    BasicProperties = Substitute.For<IBasicProperties>()
                });
            });

            msgHandler.When(x => x.HandleMessage(Arg.Any<BasicDeliverEventArgs>()))
                      .Do(callInfo => Task.Factory.StartNew(() =>
                      {
                          Thread.Sleep(1000); // Wait 1 sec so the messageInProcessCount will increase, it will block the dispose later
                          waitHandler.Set();  // Release the waitHandler so Dispose can be called
                      }));

            var consumer = new PriorityBurrowConsumer(model, msgHandler, watcher, true, 3);
            consumer.Init(SetupMockQueue(), new CompositeSubscription(), 1, "sem");
            consumer.Ready();

            // Action
            waitHandler.WaitOne();
            consumer.Dispose();


            // Assert
            watcher.Received().InfoFormat("Wait for {0} messages on queue {1} in progress", Arg.Any<object[]>());
        }

        /// <summary>
        /// Setup a mock queue to return only 1 good message
        /// </summary>
        /// <returns></returns>
        private IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>> SetupMockQueue()
        {
            var msgCount = 0;
            var queue = Substitute.For<IInMemoryPriorityQueue<GenericPriorityMessage<BasicDeliverEventArgs>>>();
            var msg = new GenericPriorityMessage<BasicDeliverEventArgs>(new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Headers = new Dictionary<string, object> { { "Priority", Encoding.UTF8.GetBytes("1") } }
                }
            }, 1);
            queue.Dequeue().Returns(msgCount == 0 ? msg : null)
                 .AndDoes(callInfo => { msgCount++; });
            
            return queue;
        }
    }
}
// ReSharper restore InconsistentNaming