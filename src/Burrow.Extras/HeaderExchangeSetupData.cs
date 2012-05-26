using System.Diagnostics;

namespace Burrow.Extras
{
    [DebuggerStepThrough]
    public class HeaderExchangeSetupData : ExchangeSetupData
    {
        public HeaderExchangeSetupData()
        {
            ExchangeType = RabbitMQ.Client.ExchangeType.Headers;
        }
    }
}
