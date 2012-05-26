namespace Burrow
{
    public interface ITunnelFactory
    {
        ITunnel Create();
        ITunnel Create(string connectionString);
        ITunnel Create(string connectionString, IRabbitWatcher watcher);
        ITunnel Create(string hostName, string virtualHost, string username, string password, IRabbitWatcher watcher);
    }
}