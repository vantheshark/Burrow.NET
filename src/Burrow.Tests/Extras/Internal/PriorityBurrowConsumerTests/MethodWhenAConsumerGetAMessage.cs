using System;
using System.Threading.Tasks;
using Burrow.Extras.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.PriorityBurrowConsumerTests
{
    [TestClass]
    public class MethodWhenAConsumerGetAMessage
    {
        [TestMethod]
        public void Should_interupt_if_the_priority_received_higher_than_priorirty_of_the_consuming_queue()
        {
            // Arrange
            var sharedBroker = new SharedEventBroker(NSubstitute.Substitute.For<IRabbitWatcher>());
            var handler = new PriorityMessageHandler(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                     NSubstitute.Substitute.For<Func<BasicDeliverEventArgs, Task>>(),
                                                     NSubstitute.Substitute.For<IRabbitWatcher>());

            var consumer = new PriorityBurrowConsumerForTest(sharedBroker, 0, NSubstitute.Substitute.For<IModel>(), handler,
                                                             NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag", false, 3);
            
            // Action
            sharedBroker.TellOthersAPriorityMessageIsHandled(NSubstitute.Substitute.For<IBasicConsumer>(), 3);
            consumer.InteruptWaitHandler.WaitOne();

            // Assert
            Assert.IsTrue(consumer.IsInterupted != null && consumer.IsInterupted.Value);
        }


        [TestMethod]
        public void Should_be_invoked_on_all_consumers_that_share_the_broker()
        {
            // Arrange
            var sharedBroker = new SharedEventBroker(NSubstitute.Substitute.For<IRabbitWatcher>());
            var handler = new PriorityMessageHandler(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                     NSubstitute.Substitute.For<Func<BasicDeliverEventArgs, Task>>(),
                                                     NSubstitute.Substitute.For<IRabbitWatcher>());

            var consumer1 = new PriorityBurrowConsumerForTest(sharedBroker, 0, NSubstitute.Substitute.For<IModel>(), handler,
                                                              NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag1", false, 3);

            var consumer2 = new PriorityBurrowConsumerForTest(sharedBroker, 1, NSubstitute.Substitute.For<IModel>(), handler,
                                                              NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag2", false, 3);

            var consumer3 = new PriorityBurrowConsumerForTest(sharedBroker, 2, NSubstitute.Substitute.For<IModel>(), handler,
                                                              NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag3", false, 3);

            // Action
            sharedBroker.TellOthersAPriorityMessageIsHandled(NSubstitute.Substitute.For<IBasicConsumer>(), 3);
            consumer1.InteruptWaitHandler.WaitOne();
            consumer2.InteruptWaitHandler.WaitOne();
            consumer3.InteruptWaitHandler.WaitOne();

            // Assert
            Assert.IsTrue(consumer1.IsInterupted != null && consumer1.IsInterupted.Value);
            Assert.IsTrue(consumer2.IsInterupted != null && consumer2.IsInterupted.Value);
            Assert.IsTrue(consumer3.IsInterupted != null && consumer3.IsInterupted.Value);
        }

        [TestMethod]
        public void Should_override_any_attemp_to_resume_previously_if_high_priority_queue_got_another_msg_in_1sec()
        {
            // Arrange
            var sharedBroker = new SharedEventBroker(NSubstitute.Substitute.For<IRabbitWatcher>());
            var handler = new PriorityMessageHandler(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                     NSubstitute.Substitute.For<Func<BasicDeliverEventArgs, Task>>(),
                                                     NSubstitute.Substitute.For<IRabbitWatcher>());

            var consumer1 = new PriorityBurrowConsumerForTest(sharedBroker, 0, NSubstitute.Substitute.For<IModel>(), handler,
                                                              NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag1", false, 3);

            var consumer2 = new PriorityBurrowConsumerForTest(sharedBroker, 1, NSubstitute.Substitute.For<IModel>(), handler,
                                                              NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag2", false, 3);

            var consumer3 = new PriorityBurrowConsumerForTest(sharedBroker, 2, NSubstitute.Substitute.For<IModel>(), handler,
                                                              NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag3", false, 3);

            // Action
            sharedBroker.TellOthersAPriorityMessageIsFinished(NSubstitute.Substitute.For<IBasicConsumer>(), 3);
            sharedBroker.TellOthersAPriorityMessageIsHandled(NSubstitute.Substitute.For<IBasicConsumer>(), 3);
            consumer1.InteruptWaitHandler.WaitOne();
            consumer2.InteruptWaitHandler.WaitOne();
            consumer3.InteruptWaitHandler.WaitOne();

            // Assert
            var tasks = new[] {
                Task.Factory.StartNew(() => Assert.IsFalse(consumer1.InteruptWaitHandler.WaitOne(3000))),
                Task.Factory.StartNew(() => Assert.IsFalse(consumer2.InteruptWaitHandler.WaitOne(3000))),
                Task.Factory.StartNew(() => Assert.IsFalse(consumer3.InteruptWaitHandler.WaitOne(3000)))
            };

            Task.WaitAll(tasks);

            Assert.IsTrue(consumer1.IsInterupted != null && consumer1.IsInterupted.Value);
            Assert.IsTrue(consumer2.IsInterupted != null && consumer1.IsInterupted.Value);
            Assert.IsTrue(consumer3.IsInterupted != null && consumer1.IsInterupted.Value);
        }
    }
}
// ReSharper restore InconsistentNaming