using System;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Burrow.Extras.Internal
{
    internal class PriorityMessageHandlerFactory : DefaultMessageHandlerFactory
    {
        public PriorityMessageHandlerFactory(IConsumerErrorHandler consumerErrorHandler, IRabbitWatcher watcher) 
            : base(consumerErrorHandler, watcher)
        {
        }

        public override IMessageHandler Create(Func<BasicDeliverEventArgs, Task> jobFactory)
        {
            return new PriorityMessageHandler(_consumerErrorHandler, jobFactory, _watcher);
        }
    }
}