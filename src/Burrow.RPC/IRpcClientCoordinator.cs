namespace Burrow.RPC
{
    public interface IRpcClientCoordinator
    {
        void SendAsync(RpcRequest request);
        RpcResponse Send(RpcRequest request);
    }
}