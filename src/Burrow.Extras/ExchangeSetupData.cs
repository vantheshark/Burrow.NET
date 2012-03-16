namespace Burrow.Extras
{
    public class ExchangeSetupData
    {
        public string ExchangeType { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }

        public ExchangeSetupData()
        {
            Durable = true;
            ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
        }
    }
}
