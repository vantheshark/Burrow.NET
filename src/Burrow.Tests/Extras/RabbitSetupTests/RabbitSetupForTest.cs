using Burrow.Extras;
using NSubstitute;
using RabbitMQ.Client;

namespace Burrow.Tests.Extras.RabbitSetupTests
{
    public class RabbitSetupForTest : RabbitSetup
    {
        public RabbitSetupForTest(string connectionString) : base(connectionString)
        {
        }

        public RabbitSetupForTest(IRabbitWatcher watcher, string connectionString) : base(watcher, connectionString)
        {
        }

        public IRabbitWatcher Watcher { get; set; }

        public static RabbitSetupForTest CreateRabbitSetup(IModel model)
        {
            var connectionFactory = Substitute.For<IDurableConnection>();
            connectionFactory.IsConnected.Returns(true);
            connectionFactory.CreateChannel().Returns(model);
            var watcher = Substitute.For<IRabbitWatcher>();
            var setup = new RabbitSetupForTest(watcher, "host=testhost;username=guest;password=guest") { DurableConnection = connectionFactory };
            setup.Watcher = watcher;
            return setup;
        }

        public void BindQueue<T>(IModel model, QueueSetupData queue, string exchangeName, string queueName, string routingKey)
        {
            base.BindQueue<T>(model, queue, exchangeName, queueName, routingKey, queue.Arguments);
        }

        public new void DeclareExchange(ExchangeSetupData exchange, IModel model, string exchangeName)
        {
            base.DeclareExchange(exchange, model, exchangeName);
        }
    }
}
