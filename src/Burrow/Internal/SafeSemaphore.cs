using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Burrow.Internal
{
    [ExcludeFromCodeCoverage]
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
                if (!_dispose)
                {
                    _semaphore.WaitOne();
                }
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
                if (!_dispose)
                {
                    _semaphore.Release();
                }
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
                if (!_dispose)
                {
                    _semaphore.Release(releaseCount);
                }
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

        private volatile bool _dispose;
        public void Dispose()
        {
            lock (_semaphore)
            {
                if (!_dispose)
                {
                    _semaphore.Dispose();
                    _dispose = true;
                }
            }
        }
    }
}
