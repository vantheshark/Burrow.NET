using System;
using System.Collections.Generic;
using System.Globalization;

namespace Burrow.RpcTestClient
{
    internal class DummyImplementation : ISomeService
    {
        public void Delete(string userId)
        {
            Global.DefaultWatcher.InfoFormat("Rpc call for msg {0} with userId {1}", "Delete", userId);
        }

        public void Save(ref SomeMessage message)
        {
        }

        public void TryParse(out string result)
        {
            result = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        public IEnumerable<SomeMessage> Get(int page, int pageSize, out int totalCount)
        {
            totalCount = 100;
            return new List<SomeMessage>{new SomeMessage()};
        }

        public IEnumerable<SomeMessage> Search(int page, SomeMessage query)
        {
            return new List<SomeMessage> { new SomeMessage() };
        }

        public Action<SomeMessage> MessageArriveEvent { get; set; }

        public SomeMessage Get(string msgId)
        {
            return new SomeMessage
            {
                Name = msgId
            };
        }

        public Action<SomeMessage> Action{ get; set; }
        public event PerformCalculation Event;
    }
}