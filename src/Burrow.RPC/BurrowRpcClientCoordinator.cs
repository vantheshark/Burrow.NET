using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Burrow.RPC
{
    public class BurrowRpcClientCoordinator<T> : IRpcClientCoordinator where T : class
    {
        private readonly string _instanceId = Guid.NewGuid().ToString();
        private readonly string _rabbitMqConnectionString;
        private readonly ITunnel _tunnel;
        private readonly IRouteFinder _routeFinder;
        private readonly ConcurrentDictionary<Guid, RpcWaitHandler> _waitHandlers = new ConcurrentDictionary<Guid, RpcWaitHandler>();
        private readonly string _requestQueueName;
        private readonly string _responseQueueName;

        internal ConcurrentDictionary<Guid, RpcWaitHandler> GetCachedWaitHandlers()
        {
            return _waitHandlers;
        }

        public BurrowRpcClientCoordinator(string rabbitMqConnectionString = null, IRouteFinder routeFinder = null, bool createRequestAndResponseQueues = true)
        {
            _rabbitMqConnectionString = InternalDependencies.RpcQueueHelper.TryGetValidConnectionString(rabbitMqConnectionString);

            _routeFinder = routeFinder ?? new BurrowRpcRouteFinder();
            _tunnel = RabbitTunnel.Factory.Create(_rabbitMqConnectionString);
            _tunnel.SetRouteFinder(_routeFinder);
            _tunnel.SetSerializer(Global.DefaultSerializer);

            var subscriptionName = typeof (T).Name + "." + _instanceId;
            _requestQueueName = _routeFinder.FindQueueName<RpcRequest>(typeof(T).Name);
            _responseQueueName = _routeFinder.FindQueueName<RpcResponse>(subscriptionName);

            Init(createRequestAndResponseQueues, subscriptionName);
        }

        private void Init(bool createRequestAndResponseQueues, string subscriptionName)
        {
            Action<IModel> createRequestAndResponseQueuesAction = channel =>
            {
                IDictionary arguments = new Dictionary<string, object>();
                channel.QueueDeclare(_responseQueueName, true, false, true /* response queue will be deleted if client disconnected */, arguments);
                if (createRequestAndResponseQueues)
                {
                    channel.QueueDeclare(_requestQueueName, true, false, false, arguments);
                    var requestExchange = _routeFinder.FindExchangeName<RpcRequest>();
                    if (!string.IsNullOrEmpty(requestExchange))
                    {
                        channel.ExchangeDeclare(requestExchange, ExchangeType.Direct, true, false, null);
                        channel.QueueBind(_requestQueueName, requestExchange, _requestQueueName /* Routing key is always same as requestQueueName*/);
                    }
                }
            };

            InternalDependencies.RpcQueueHelper.CreateQueues(_rabbitMqConnectionString, createRequestAndResponseQueuesAction);
            
            _tunnel.Subscribe<RpcResponse>(subscriptionName, ReceiveResponse);
        }

        internal void ReceiveResponse(RpcResponse rpcResponse)
        {
            if (!_waitHandlers.ContainsKey(rpcResponse.RequestId))
            {
                Global.DefaultWatcher.WarnFormat("RequestId {0} not found", rpcResponse.RequestId);
                return;
            }

            var waitHandler = _waitHandlers[rpcResponse.RequestId];
            waitHandler.Response = rpcResponse;
            waitHandler.WaitHandle.Set();
        }

        public virtual void SendAsync(RpcRequest request)
        {
            request.ResponseAddress = null; // Don't need response
            _tunnel.Publish(request, _requestQueueName);
        }

        public virtual RpcResponse Send(RpcRequest request)
        {
            request.ResponseAddress = _responseQueueName;
            
            var waitHandler = new RpcWaitHandler();
            _waitHandlers[request.Id] = waitHandler;
            _tunnel.Publish(request, _requestQueueName);

            var timeToWait = request.UtcExpiryTime.HasValue && DateTime.UtcNow < request.UtcExpiryTime.Value
                           ? request.UtcExpiryTime.Value.Subtract(DateTime.UtcNow)
                           : new TimeSpan(int.MaxValue);

            if (waitHandler.WaitHandle.WaitOne(timeToWait))
            {
                var response = waitHandler.Response;
                _waitHandlers.TryRemove(request.Id, out waitHandler);
                return response;
            }
            throw new TimeoutException(string.Format("Has been waiting for {0} seconds but no response from server", timeToWait.TotalSeconds));
        }
    }
}