using System;
using System.Collections.Generic;
using Burrow.RPC;

namespace Burrow.RpcTestClient
{
    public delegate int PerformCalculation(int x, int y);

    public interface ISomeService
    {
        [Async]
        void Delete(string userId);
        
        [Async]
        void Save(ref SomeMessage message);
        
        [RpcTimeToLive(100)]
        void TryParse(out string result);
        
        IEnumerable<SomeMessage> Get(int page, int pageSize, out int totalCount);
        
        IEnumerable<SomeMessage> Search(int page, SomeMessage query);
        
        [Async]
        SomeMessage Get(string msgId);
        
        Action<SomeMessage> Action { get; set; }

        event PerformCalculation Event;
    }
}
