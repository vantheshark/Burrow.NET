
namespace Burrow.RPC
{
    /// <summary>
    /// A default rpc route finder using default built-in "empty" exchange for sending requests/response directly to the queue name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultRpcRouteFinder<T> : IRpcRouteFinder where T : class
    {
        protected readonly string _requestQueueName;
        protected readonly string _clientName;

        /// <summary> 
        /// </summary>
        /// <param name="requestQueueName">if provided, it will be used as the request queue name. Otherwise the default conventional queue will be used</param>
        /// <param name="clientName">anything that can make the response queue unique</param>
        public DefaultRpcRouteFinder(string requestQueueName = null, string clientName = null)
        {
            _requestQueueName = requestQueueName;
            _clientName = clientName;
        }

        public virtual bool CreateExchangeAndQueue
        {
            get { return true; }
        }

        public virtual string RequestExchangeName
        {
            get { return string.Empty; }
        }

        public virtual string RequestExchangeType
        {
            get { return null; }
        }

        public virtual string RequestQueue
        {
            get
            {
                return string.IsNullOrEmpty(_requestQueueName)
                    ? string.Format("Burrow.Queue.Rpc.{0}.Requests", typeof(T).Name)
                    : _requestQueueName; 
            }
        }

        public virtual string UniqueResponseQueue
        {
            get { return string.IsNullOrEmpty(_clientName) 
                       ? string.Format("Burrow.Queue.Rpc.{0}.Responses", typeof(T).Name)
                       : string.Format("Burrow.Queue.Rpc.{0}.{1}.Responses", _clientName, typeof(T).Name); }
        }
    }
}