using RabbitMQ.Client;

namespace Burrow.RPC
{
    /// <summary>
    /// A default rpc route finder using Fanout Exchange for the Requests
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultFanoutRpcRequestRouteFinder<T> : DefaultRpcRouteFinder<T> where T : class
    {
        private readonly string _serverId;

        public DefaultFanoutRpcRequestRouteFinder(string serverId, string requestQueueName = null, string rpcClientName = null)
            : this(requestQueueName, rpcClientName)
        {
            _serverId = serverId;
        }

        public DefaultFanoutRpcRequestRouteFinder(string requestQueueName = null, string rpcClientName = null)
            : base(requestQueueName, rpcClientName)
        {
        }

        public new virtual string RequestExchangeName
        {
            get { return string.Format("Burrow.Exchange.Rpc.{0}.Requests", typeof (T).Name); }
        }

        public new string RequestExchangeType
        {
            get { return ExchangeType.Fanout; }
        }

        public override string RequestQueue
        {
            get { 
                return string.IsNullOrEmpty(_requestQueueName)
                    ? string.Format("Burrow.Queue.Rpc.{0}.{1}.Requests", _serverId, typeof(T).Name).Replace("..", ".") 
                    : _requestQueueName; 
            }
        }
    }
}