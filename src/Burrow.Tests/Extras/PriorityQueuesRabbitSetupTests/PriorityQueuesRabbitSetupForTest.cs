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

        public ConnectionFactory ConnectionFactory
        {
            set { _connectionFactory = value; }
        }

        public static PriorityQueuesRabbitSetupForTest CreateRabbitSetup(IModel model)
        {
            var connection = Substitute.For<IConnection>();
            connection.CreateModel().Returns(model);
            
            var connectionFactory = Substitute.For<ConnectionFactory>();
            connectionFactory.CreateConnection().Returns(connection);
            var setup = new PriorityQueuesRabbitSetupForTest("") { ConnectionFactory = connectionFactory };
            return setup;
        }
    }
}
