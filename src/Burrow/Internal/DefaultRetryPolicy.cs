using System;
using System.Threading;
using System.Threading.Tasks;

namespace Burrow.Internal
{
    public class DefaultRetryPolicy : IRetryPolicy
    {
        private readonly int _maxDelayTime;
        private const int DelayGap = 1000;

        public DefaultRetryPolicy(int maxDelayTime = 5 * 60 * 1000 /* 5 minutes */)
        {
            _maxDelayTime = maxDelayTime;
            DelayTime = 0;
        }

        public int DelayTime { get; private set; }

        public void WaitForNextRetry(Action retryingAction)
        {
            var t = new Task(() => {
                Thread.Sleep(DelayTime);
                DelayTime = DelayTime == 0
                          ? DelayGap
                          : DelayTime * 2;

                if (DelayTime > _maxDelayTime)
                {
                    DelayTime = _maxDelayTime;
                }

                retryingAction();
            });
            t.Start();
        }

        public void Reset()
        {
            DelayTime = 0;
        }
    }
}
