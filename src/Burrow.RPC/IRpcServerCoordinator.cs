namespace Burrow.RPC
{
    public interface IRpcServerCoordinator
    {
        void Start();
        void HandleMesage(RpcRequest request);
    }
}