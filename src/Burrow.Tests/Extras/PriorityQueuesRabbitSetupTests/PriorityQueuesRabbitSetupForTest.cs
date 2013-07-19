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
            var connection = Substitute.For<IConnection>();
            connection.CreateModel().Returns(model);
            
            var connectionFactory = Substitute.For<ConnectionFactory>();
            connectionFactory.CreateConnection().Returns(connection);
            var setup = watcher == null
                      ? new PriorityQueuesRabbitSetupForTest("") { ConnectionFactory = connectionFactory }
                      : new PriorityQueuesRabbitSetupForTest(watcher, "") { ConnectionFactory = connectionFactory };
            return setup;
        }
    }
}
