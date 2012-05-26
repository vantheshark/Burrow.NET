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
    public class MethodWhenAConsumerFinishedAMessage
    {
        [TestMethod]
        public void Should_resume_after_one_second_if_the_priority_received_higher_than_priorirty_of_the_consuming_queue()
        {
            // Arrange
            var sharedBroker = new SharedEventBroker(NSubstitute.Substitute.For<IRabbitWatcher>());
            var handler = new PriorityMessageHandler(NSubstitute.Substitute.For<IConsumerErrorHandler>(),
                                                     NSubstitute.Substitute.For<Func<BasicDeliverEventArgs, Task>>(),
                                                     NSubstitute.Substitute.For<IRabbitWatcher>());

            var consumer = new PriorityBurrowConsumerForTest(sharedBroker, 0, NSubstitute.Substitute.For<IModel>(), handler,
                                                             NSubstitute.Substitute.For<IRabbitWatcher>(), "consumerTag", false, 3);
            
            // Action
            sharedBroker.TellOthersAPriorityMessageIsFinished(NSubstitute.Substitute.For<IBasicConsumer>(), 3);
            consumer.ResumeWaitHandler.WaitOne();

            // Assert
            Assert.IsTrue(consumer.IsInterupted != null && !consumer.IsInterupted.Value);
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
            sharedBroker.TellOthersAPriorityMessageIsFinished(NSubstitute.Substitute.For<IBasicConsumer>(), 3);
            consumer1.ResumeWaitHandler.WaitOne();
            consumer2.ResumeWaitHandler.WaitOne();
            consumer3.ResumeWaitHandler.WaitOne();

            // Assert
            Assert.IsTrue(consumer1.IsInterupted != null && !consumer1.IsInterupted.Value);
            Assert.IsTrue(consumer2.IsInterupted != null && !consumer2.IsInterupted.Value);
            Assert.IsTrue(consumer3.IsInterupted != null && !consumer3.IsInterupted.Value);
        }
    }
}
// ReSharper restore InconsistentNaming