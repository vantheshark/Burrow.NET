using System;
using System.Threading;

namespace Burrow.Internal
{
    public class SafeSemaphore : IDisposable
    {
        private readonly IRabbitWatcher _watcher;
        private readonly Semaphore _semaphore;

        public SafeSemaphore(IRabbitWatcher watcher, int initialCount, int maximumCount)
        {
            _watcher = watcher;
            _semaphore = new Semaphore(initialCount, maximumCount);
        }

        public SafeSemaphore(IRabbitWatcher watcher, int initialCount, int maximumCount, string name)
        {
            _watcher = watcher;
            _semaphore = new Semaphore(initialCount, maximumCount, name);
        }

        public void WaitOne()
        {
            try
            {
                _semaphore.WaitOne();
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        public void Release()
        {
            try
            {
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        public void Release(int releaseCount)
        {
            try
            {
                _semaphore.Release(releaseCount);
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        public void Close()
        {
            try
            {
                _semaphore.Close();
            }
            catch (Exception ex)
            {
                _watcher.Error(ex);
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
