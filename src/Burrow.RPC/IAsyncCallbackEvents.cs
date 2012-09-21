namespace Burrow.RPC
{
    public interface IAsyncCallbackEvents
    {
        [Async]
        int Multiple(int x, int y);
    }
}