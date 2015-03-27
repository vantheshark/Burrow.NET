using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Burrow.Extras.Internal;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Burrow.Tests.Extras.Internal.InMemoryPriorityQueueTests
{
    [TestFixture]
    public class MethodEnqueue
    {
        [Test]
        public void Should_add_item_to_queue_if_not_reach_match_size()
        {
            // Arrange
            var queue = new InMemoryPriorityQueue<GenericPriorityMessage<string>>(2, new PriorityComparer<GenericPriorityMessage<string>>());

            // Action
            queue.Enqueue(new GenericPriorityMessage<string>("", 1));

            // Assert
            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void Should_block_the_adding_thread_if_queue_is_full()
        {
            // Arrange
            var test = true;
            var waitFirstEnqueue = new AutoResetEvent(false);
            
            var queue = new InMemoryPriorityQueue<GenericPriorityMessage<string>>(2, new PriorityComparer<GenericPriorityMessage<string>>());

            // Action
            Task.Factory.StartNew(() =>
            {
                while (test)
                {
                    queue.Enqueue(new GenericPriorityMessage<string>("", 1));
                    waitFirstEnqueue.Set();
                }
                //Block
            });
            Assert.IsTrue(waitFirstEnqueue.WaitOne(1000));

            queue.Dequeue();
            queue.Dequeue();

            // Assert
            test = false;
            queue.Close();
        }

        [Test, ExpectedException(typeof(EndOfStreamException))]
        public void Should_throw_exception_if_queue_is_closed()
        {
            // Arrange
            var queue = new InMemoryPriorityQueue<GenericPriorityMessage<string>>(2, new PriorityComparer<GenericPriorityMessage<string>>());

            // Action
            queue.Close();
            queue.Enqueue(new GenericPriorityMessage<string>("", 1));
        }
    }
}
// ReSharper restore InconsistentNaming