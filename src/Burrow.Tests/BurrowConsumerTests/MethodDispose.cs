using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.BurrowConsumerTests
{
    [TestFixture]
    public class MethodDispose
    {
        [Test]
        public void Can_called_many_times()
        {
            // Arrange
            var consumer = new BurrowConsumer(Substitute.For<IModel>(), Substitute.For<IMessageHandler>(),
                                              Substitute.For<IRabbitWatcher>(), false, 3) { ConsumerTag = "ConsumerTag" };

            // Action
            consumer.Dispose();
            consumer.Dispose();
        }

        [Test]
        public void Can_wait_until_messageInProcess_down_to_0()
        {
            // Arrange
            var fireHandlingComplete = false;
            var waitHandler = new AutoResetEvent(false);
            var model = Substitute.For<IModel>();
            model.IsOpen.Returns(true);
            var msgHandler = Substitute.For<IMessageHandler>();
            

            var watcher = Substitute.For<IRabbitWatcher>();
            watcher.IsDebugEnable.Returns(true);
            //To decrease the messagages in progress so it doesn't have to wait when dispose at the end
            watcher.When(x => x.InfoFormat(Arg.Any<string>(), Arg.Any<object[]>()))
                    .Do(callInfo => Task.Factory.StartNew(() =>
                    {
                        if (fireHandlingComplete) return;
                        //When print log for waiting, fire HandlingComplete on msgHandler to decrease the messageInProcessCount
                        Thread.Sleep(2000);
                        msgHandler.HandlingComplete += Raise.Event<MessageHandlingEvent>(new BasicDeliverEventArgs
                        {
                            BasicProperties = Substitute.For<IBasicProperties>(),
                            ConsumerTag = "ConsumerTag"
                        });
                        fireHandlingComplete = true;
                    }));


            msgHandler.When(x => x.HandleMessage(Arg.Any<BasicDeliverEventArgs>()))
                      .Do(callInfo => Task.Factory.StartNew(() =>
                      {
                          Thread.Sleep(1000); // Wait 1 sec so the messageInProcessCount will increase, it will block the dispose later
                          waitHandler.Set();  // Release the waitHandler so Dispose can be called
                      }));

            var consumer = new BurrowConsumer(model, msgHandler, watcher, true, 3) { ConsumerTag = "ConsumerTag" };
            Subscription.OutstandingDeliveryTags[consumer.ConsumerTag] = new List<ulong>();
            // Action
            // Enqueue only 1 msg
            consumer.Queue.Enqueue(new BasicDeliverEventArgs
            {
                BasicProperties = Substitute.For<IBasicProperties>(),
                ConsumerTag = "ConsumerTag"
            });
            Assert.IsTrue(waitHandler.WaitOne(1500));
            consumer.Dispose();


            // Assert
            watcher.Received().InfoFormat("Wait for {0} messages in progress", Arg.Any<object[]>());
        }
    }
}
// ReSharper restore InconsistentNaming