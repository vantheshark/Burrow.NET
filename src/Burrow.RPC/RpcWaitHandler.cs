using System.Threading;

namespace Burrow.RPC
{
    /// <summary>
    /// This class is used to block the client thread code when wait for the response from server if the method is sync
    /// </summary>
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