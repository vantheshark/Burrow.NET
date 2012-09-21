using System.Threading;

namespace Burrow.RPC
{
    public class RpcWaitHandler
    {
        public AutoResetEvent WaitHandle { get; private set; }
        public RpcResponse Response { get; set; }

        public RpcWaitHandler()
        {
            WaitHandle = new AutoResetEvent(false);
        }
    }
}