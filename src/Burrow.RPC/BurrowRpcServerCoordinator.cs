using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Burrow.RPC
{
    /// <summary>
    /// Default impelementation of IRpcServerCoordinator which can handle RPC request by subscribing to a request queue in RabbitMQ server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BurrowRpcServerCoordinator<T> : IRpcServerCoordinator where T : class
    {
        private readonly string _serverId;
        private readonly T _realInstance;
        private readonly string _rabbitMqConnectionString;
        private readonly IRpcRouteFinder _routeFinder;
        private ITunnel _tunnel;
        
        public BurrowRpcServerCoordinator(T realInstance, string requestQueueName, string rabbitMqConnectionString = null, string serverId = null)
            : this(realInstance, new DefaultRpcRouteFinder<T>(requestQueueName), rabbitMqConnectionString, serverId)
        {
        }

        public BurrowRpcServerCoordinator(T realInstance, IRpcRouteFinder routeFinder, string rabbitMqConnectionString = null, string serverId = null)
        {
            _rabbitMqConnectionString = InternalDependencies.RpcQueueHelper.TryGetValidConnectionString(rabbitMqConnectionString);
            if (realInstance == null)
            {
                throw new ArgumentNullException("realInstance");
            }

            if (routeFinder == null)
            {
                throw new ArgumentNullException("routeFinder");
            }

            _realInstance = realInstance;
            _routeFinder = routeFinder;
            _serverId = serverId;
        }

        private void Init()
        {
            _tunnel = RabbitTunnel.Factory.Create(_rabbitMqConnectionString);
            _tunnel.SetRouteFinder(new RpcRouteFinderAdapter(_routeFinder));
            _tunnel.SetSerializer(Global.DefaultSerializer);

            if ( _routeFinder.CreateExchangeAndQueue)
            {
                var routingKey = _routeFinder.RequestQueue;
                var requestQueueName = _routeFinder.RequestQueue;
                var requestExchange = _routeFinder.RequestExchangeName;

                Action<IModel> createRequestQueues = channel =>
                {
                    var arguments = new Dictionary<string, object>();
                    var autoDeleteLoadBalanceRequestQueue = !string.IsNullOrEmpty(_serverId) && !string.IsNullOrEmpty(requestExchange);

                    channel.QueueDeclare(requestQueueName, true, false, autoDeleteLoadBalanceRequestQueue, arguments);
                    if (!string.IsNullOrEmpty(requestExchange))
                    {
                        channel.ExchangeDeclare(requestExchange, _routeFinder.RequestExchangeType, true, false, null);
                        channel.QueueBind(requestQueueName, requestExchange, routingKey);
                    }
                };
                InternalDependencies.RpcQueueHelper.CreateQueues(_rabbitMqConnectionString, createRequestQueues);
            }
        }

        public void Start()
        {
            Init();
            _tunnel.SubscribeAsync<RpcRequest>(_serverId ?? typeof(T).Name, HandleMesage);
        }

        public void HandleMesage(RpcRequest msg)
        {
            if (msg.UtcExpiryTime != null && msg.UtcExpiryTime < DateTime.UtcNow)
            {
                Global.DefaultWatcher.WarnFormat("Msg {0}.{1} from {2} has been expired", msg.DeclaringType, msg.MethodName, msg.ResponseAddress);
                return;
            }

            var response = new RpcResponse
            {
                RequestId = msg.Id,
            };
            try
            {
                var methodInfo = InternalDependencies.MethodMatcher.Match<T>(msg);
                if (methodInfo == null)
                {
                    throw new Exception(string.Format("Could not find a match member of type {0} for method {1} of {2}", msg.MemberType.ToString(), msg.MethodName, msg.DeclaringType));
                }

                object[] parameters = msg.Params.Values.ToArray();
                response.ReturnValue = methodInfo.Invoke(_realInstance, parameters);
                var keys = msg.Params.Keys.ToArray();

                for (int i = 0; i < msg.Params.Count; i++)
                {
                    msg.Params[keys[i]] = parameters[i];
                }
                response.ChangedParams = msg.Params;

            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }

            if (!string.IsNullOrEmpty(msg.ResponseAddress))
            {
                _tunnel.Publish(response, msg.ResponseAddress);
            }
        }
    }
}