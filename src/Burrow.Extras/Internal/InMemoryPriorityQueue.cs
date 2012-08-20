using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using C5;

namespace Burrow.Extras.Internal
{
    public interface IInMemoryPriorityQueue<T> where T : IPriorityMessage
    {
        void Close();
        T Dequeue();
        void Enqueue(T item);
        int Count { get; }
        void DeleteAll(Predicate<T> match);
    }

    internal class InMemoryPriorityQueue<T> : IInMemoryPriorityQueue<T> where T : IPriorityMessage
    {
        protected volatile bool Closing;
        private readonly long _maxSize;
        protected IntervalHeap<T> PriorityQueue;


        public void DeleteAll(Predicate<T> match)
        {
            lock (PriorityQueue)
            {
                Closing = false;
                var all = PriorityQueue.ToList();
                all.RemoveAll(match);

                // Clear all items
                while(PriorityQueue.Count > 0)
                {
                    PriorityQueue.DeleteMax();
                }

                // Add the rest
                PriorityQueue.AddAll(all);
                Monitor.PulseAll(PriorityQueue);
            }
        }

        public InMemoryPriorityQueue(long maxSize, IComparer<T> comparer)
        {
            _maxSize = maxSize;
            PriorityQueue = new IntervalHeap<T>(comparer);
        }

        public void Close()
        {
            lock (PriorityQueue)
            {
                Closing = true;
                Monitor.PulseAll(PriorityQueue);
            }
        }

        private void EnsureIsOpen()
        {
            if (Closing)
            {
                Monitor.PulseAll(PriorityQueue);
                throw new EndOfStreamException("PriorityQueue closed");
            }
        }

        public T Dequeue()
        {
            lock (PriorityQueue)
            {
                while (PriorityQueue.Count == 0)
                {
                    Monitor.Wait(PriorityQueue);
                }
                EnsureIsOpen();
                T local = PriorityQueue.DeleteMax();
                if (PriorityQueue.Count == (_maxSize - 1))
                {
                    Monitor.PulseAll(PriorityQueue);
                }
                return local;
            }
        }
        public void Enqueue(T item)
        {
            lock (PriorityQueue)
            {
                while (PriorityQueue.Count() >= _maxSize)
                {
                    Monitor.Wait(PriorityQueue);
                }
                EnsureIsOpen();
                PriorityQueue.Add(item);
                if (PriorityQueue.Count == 1)
                {
                    Monitor.PulseAll(PriorityQueue);
                }
            }
        }

        public int Count
        {
            get
            {
                lock (PriorityQueue)
                {
                    return PriorityQueue.Count;
                }
            }
        }
    }
}