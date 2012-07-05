using System.Threading;
using System.Threading.Tasks;

namespace Burrow
{
    /// <summary>
    /// This class try to find available Threads in Pool to determine whether using ThreadPool or Dedicated Thread by returning a proper TaskCreationOptions
    /// In Win32, the MaxWorker is 1023 where the number is quite high in Win64 bit. So if your app is running as a 64 bit app, you properly need to call ThreadPool.SetMaxThreads()
    /// to a proper number or simply change the Global.DefaultTaskCreationOptionsProvider func to return TaskCreationOptions.LongRunning which eventually make TPL create dedicated Thread 
    /// instead of queue a worker in ThreadPool
    /// </summary>
    internal class DefaultTaskCreationOptionProvider
    {
        private readonly int _availableWorkerThreshold;

        public DefaultTaskCreationOptionProvider(int availableWorkerThreshold = 4 /*If there are 4 threads available in pool, use ThreadPool otherwise create a dedicated Thread*/)
        {
            _availableWorkerThreshold = availableWorkerThreshold;
        }

        public TaskCreationOptions GetOptions()
        {
            int worker ;
            int ioCompletion;           
            
            ThreadPool.GetAvailableThreads(out worker, out ioCompletion);
#if DEBUG
            Global.DefaultWatcher.DebugFormat("Available workers in ThreadPool: {0}. The number of available asynchronous I/O threads: {1} ", worker, ioCompletion);
#endif
            if (worker > _availableWorkerThreshold)
            {
                return TaskCreationOptions.PreferFairness;
            }
            
            //NOTE: TO make the TPL library create a dedicated thread instead of using ThreadPool
            return TaskCreationOptions.LongRunning;
        }
    }
}
