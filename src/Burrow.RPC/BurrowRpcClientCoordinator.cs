using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Burrow.RPC
{
    /// <summary>
    /// Default implementation of IRpcClientCoordinator which sends requests to RabbitMQ server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BurrowRpcClientCoordinator<T> : IRpcClientCoordinator where T : class
    {
        private readonly string _rabbitMqConnectionString;
        private readonly ITunnel _tunnel;
        private readonly IRpcRouteFinder _routeFinder;
        private readonly ConcurrentDictionary<Guid, RpcWaitHandler> _waitHandlers = new ConcurrentDictionary<Guid, RpcWaitHandler>();

        internal ConcurrentDictionary<Guid, RpcWaitHandler> GetCachedWaitHandlers()
        {
            return _waitHandlers;
        }

        public BurrowRpcClientCoordinator(string rabbitMqConnectionString = null, IRpcRouteFinder routeFinder = null)
        {
            _rabbitMqConnectionString = InternalDependencies.RpcQueueHelper.TryGetValidConnectionString(rabbitMqConnectionString);

            _routeFinder = routeFinder ?? new DefaultRpcRouteFinder<T>();
            _tunnel = RabbitTunnel.Factory.Create(_rabbitMqConnectionString);
            _tunnel.SetRouteFinder(new RpcRouteFinderAdapter(_routeFinder));
            _tunnel.SetSerializer(Global.DefaultSerializer);

            var subscriptionName = typeof(T).Name;
            Init(subscriptionName);
        }

        private void Init(string subscriptionName)
        {
            Action<IModel> createRequestAndResponseQueuesAction = channel =>
            {
                var arguments = new Dictionary<string, object>();
                channel.QueueDeclare(_routeFinder.UniqueResponseQueue, true, false, true /* response queue will be deleted if client disconnected */, arguments);
                if (_routeFinder.CreateExchangeAndQueue)
                {
                    channel.QueueDeclare(_routeFinder.RequestQueue, true, false, false, arguments);
                    var requestExchange = _routeFinder.RequestExchangeName;
                    if (!string.IsNullOrEmpty(requestExchange))
                    {
                        channel.ExchangeDeclare(requestExchange, _routeFinder.RequestExchangeType, true, false, null);
                        channel.QueueBind(_routeFinder.RequestQueue, requestExchange, _routeFinder.RequestQueue /* Routing key is always same as requestQueueName*/);
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
            _tunnel.Publish(request, _routeFinder.RequestQueue);
        }

        public virtual RpcResponse Send(RpcRequest request)
        {
            request.ResponseAddress = _routeFinder.UniqueResponseQueue;
            
            var waitHandler = new RpcWaitHandler();
            _waitHandlers[request.Id] = waitHandler;
            _tunnel.Publish(request, _routeFinder.RequestQueue);

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