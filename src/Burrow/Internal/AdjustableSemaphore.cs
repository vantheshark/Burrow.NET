using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
namespace Burrow.Internal
{
    public class AdjustableSemaphore : IDisposable
    {
        public int MaximumCount { get; private set; }
        public volatile int Count;
        private readonly List<WaitHandle> _waitHandlers;
        public volatile List<int> WaitIndex; 
        private object sync = new object();
        
        public void Dispose()
        {
            _waitHandlers.ForEach(h => h.Dispose());
        }

        public AdjustableSemaphore(int count, int maximumCount)
        {
            if (count > maximumCount)
            {
                throw new ArgumentException("'count' must be less than or equal 'maximumCount'", "count");
            }

            Count = count;
            MaximumCount = maximumCount;
            _waitHandlers = new List<WaitHandle>(maximumCount + 1);
            for(int i=0; i< maximumCount; i++)
            {
                _waitHandlers.Add(new AutoResetEvent(true));
            }
            _waitHandlers.Insert(0, new AutoResetEvent(false));
            WaitIndex = new List<int>();

        }

        public void WaitOne()
        {
            int index;
            do
            {
                index = WaitHandle.WaitAny(_waitHandlers.Take(Count + 1).ToArray(), 1000);
            } while (index == WaitHandle.WaitTimeout);
            WaitIndex.Add(index);
        }

        public void Release()
        {
            ((AutoResetEvent) _waitHandlers[WaitIndex.First()]).Set();
            WaitIndex.RemoveAt(0);
        }

        public int Adjust(int volume)
        {
            lock (sync)
            {
                var oldCount = Count;
                Count += volume;
                Count = Math.Min(Count, MaximumCount);
                Count = Math.Max(0, Count);
                return Count - oldCount;
            }
        }
    }

    //public class AdjustableSemaphore : IDisposable
    //{
    //    private readonly SemaphoreSlim _semaphore;
    //    private readonly CountdownEvent _waitHandler = new CountdownEvent(0);
        
    //    public int MaximumCount { get; private set; }
    //    private readonly object _sync = new object();

    //    public AdjustableSemaphore(int count, int maximumCount)
    //    {
    //        if (count > maximumCount)
    //        {
    //            throw new ArgumentException("'count' must be less than or equal 'maximumCount'", "count");
    //        }
    //        MaximumCount = maximumCount;

    //        _semaphore = new SemaphoreSlim(count, MaximumCount);
            
    //    }

    //    public void WaitOne()
    //    {
    //        if (_semaphore.CurrentCount == 0)
    //        {
    //            if (_waitHandler.IsSet)
    //            {
    //                _waitHandler.Reset(1);
    //            }
    //            else
    //            {
    //                _waitHandler.AddCount(1);    
    //            }
    //            _waitHandler.Wait();
    //        }
            
    //        _semaphore.Wait();
    //    }

    //    public void Release()
    //    {
    //        try
    //        {
    //            _semaphore.Release();
    //        }
    //        catch(Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }

    //    public int Adjust(int volume)
    //    {
    //        lock (_sync)
    //        {
    //            int vol = volume;
    //            if (volume > 0)
    //            {
    //                for(int i=0; i< volume; i++)
    //                {
    //                    _semaphore.Release();
    //                    _waitHandler.Signal();
    //                }
    //            }

    //            if (volume < 0)
    //            {
    //                vol = -volume;
    //                if (_waitHandler.IsSet)
    //                {
    //                    _waitHandler.Reset(vol);
    //                }
    //                else
    //                {
    //                    _waitHandler.AddCount(vol);
    //                }
    //            }
    //            return vol;
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        _semaphore.Dispose();
    //    }
    //}
}
