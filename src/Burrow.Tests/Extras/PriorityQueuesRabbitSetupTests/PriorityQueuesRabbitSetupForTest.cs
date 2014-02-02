using Burrow.Extras;
using NSubstitute;
using RabbitMQ.Client;

namespace Burrow.Tests.Extras.PriorityQueuesRabbitSetupTests
{
    public class PriorityQueuesRabbitSetupForTest : PriorityQueuesRabbitSetup
    {
        public PriorityQueuesRabbitSetupForTest(string connectionString) : base(connectionString)
        {
        }

        public PriorityQueuesRabbitSetupForTest(IRabbitWatcher watcher, string connectionString) : base(watcher, connectionString)
        {
        }

        public static PriorityQueuesRabbitSetupForTest CreateRabbitSetup(IModel model, IRabbitWatcher watcher = null)
        {
            var connectionFactory = Substitute.For<IDurableConnection>();
            connectionFactory.IsConnected.Returns(true);
            connectionFactory.CreateChannel().Returns(model);
            var setup = watcher == null
                      ? new PriorityQueuesRabbitSetupForTest("") { DurableConnection = connectionFactory }
                      : new PriorityQueuesRabbitSetupForTest(watcher, "") { DurableConnection = connectionFactory };
            return setup;
        }
    }
}
