using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;

namespace Burrow.RPC
{
    internal class RpcClientInterceptor : IInterceptor
    {
        private readonly IRpcClientCoordinator _clientCoordinator;
        private readonly List<IMethodFilter> _methodFilters;
        internal static IMethodMatcher MethodMatcher = new MethodMatcher();

        public RpcClientInterceptor(IRpcClientCoordinator clientCoordinator, params IMethodFilter[] methodFilters)
        {
            _clientCoordinator = clientCoordinator;
            _methodFilters = new List<IMethodFilter>((methodFilters ?? new IMethodFilter[0]).Union(new [] {new DefaultMethodFilter()}));
            _methodFilters.RemoveAll(filter => filter == null);
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            var attributes = method.GetCustomAttributes(true).Select(x => x as Attribute).ToArray();
            var isAsync = _methodFilters.Any(filter => filter.IsAsync(method, attributes));
            _methodFilters.ForEach(filter => filter.CheckValid(method, attributes, isAsync));

            var @params = method.GetParameters().ToList();

            var args = new Dictionary<string, object>();
            for(int i=0 ; i < invocation.Arguments.Length; i++)
            {
                args.Add(@params[i].Name, invocation.Arguments[i]);
            }

            var request = new RpcRequest
            {
                Params = args,
                MemberType = method.MemberType,
                MethodName = method.Name,
                MethodSignature = MethodMatcher.GetMethodSignature(method),
                DeclaringType = method.DeclaringType.FullName,
            };

            var timeToLiveAttribute = attributes.LastOrDefault(x => x is RpcTimeToLiveAttribute);
            if (timeToLiveAttribute != null)
            {
                var att = (RpcTimeToLiveAttribute) timeToLiveAttribute;
                if (att.Seconds > 0)
                {
                    request.UtcExpiryTime = DateTime.UtcNow.AddSeconds(att.Seconds);
                }
            }

            if (isAsync)
            {
                _clientCoordinator.SendAsync(request);
                return;
            }

            var response = _clientCoordinator.Send(request);

            if (response == null)
            {
                throw new Exception("RpcResponse is null for some reason. It's always expected to be something not null, probably there's something wrong with the bloody Coordinator");
            }

            MapResponseResult(invocation, @params, response);
        }

        private static void MapResponseResult(IInvocation invocation, List<ParameterInfo> @params, RpcResponse response)
        {
            if (response.Exception != null)
            {
                throw response.Exception;
            }

            if (invocation.Method.ReturnType != typeof (void))
            {
                invocation.ReturnValue = response.ReturnValue;
            }

            var outParams = @params.Where(x => x.IsOut).Select(x => x.Name);
            var missingOutValue = outParams.FirstOrDefault(param => !(response.ChangedParams ?? new Dictionary<string, object>()).ContainsKey(param));
            if (missingOutValue != null)
            {
                throw new Exception(string.Format("RpcResponse does not contain the modified value for param {0} which is an 'out' param. Probably there's something wrong with the bloody Coordinator", missingOutValue));
            }

            for (var i = 0; i < @params.Count; i++)
            {
                if (@params[i].IsOut)
                {
                    invocation.SetArgumentValue(i, response.ChangedParams[@params[i].Name]);
                }
                else if (@params[i].ParameterType.IsByRef &&
                         response.ChangedParams.ContainsKey(@params[i].Name))
                {
                    invocation.SetArgumentValue(i, response.ChangedParams[@params[i].Name]);
                }
            }
        }
    }
}