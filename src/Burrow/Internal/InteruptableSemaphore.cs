using System;
using System.Threading;

namespace Burrow.Internal
{
    public class InteruptableSemaphore : IDisposable
    {
        private readonly Semaphore _semaphore;
        private readonly ManualResetEvent _manualResetEvent;
        private volatile bool _isInterupted;
        public InteruptableSemaphore(int initialCount, int maximumCount)
        {
            _semaphore = new Semaphore(initialCount, maximumCount);
            _manualResetEvent = new ManualResetEvent(true);
        }
            
        public void WaitOne()
        {
            _manualResetEvent.WaitOne();
            _semaphore.WaitOne();
        }

        public void Release()
        {
            _semaphore.Release();
        }

        public int Interupt()
        {
            _manualResetEvent.Reset();
            if (_isInterupted)
            {
                return 0;
            }
            _isInterupted = true;
            return 1;
        }

        public int Resume()
        {
            _manualResetEvent.Set();
            if (!_isInterupted)
            {
                return 0;
            }
            _isInterupted = false;
            return 1;
        }

        public void Dispose()
        {
            _semaphore.Dispose();
            _manualResetEvent.Dispose();
        }
    }
}
