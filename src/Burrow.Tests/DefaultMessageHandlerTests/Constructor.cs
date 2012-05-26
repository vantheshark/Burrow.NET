using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client.Events;
using NSubstitute;

// ReSharper disable InconsistentNaming
namespace Burrow.Tests.DefaultMessageHandlerTests
{
    [TestClass]
    public class Constructor
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_consumerErrorHandler()
        {
            new DefaultMessageHandler(null, Substitute.For<Func<BasicDeliverEventArgs, Task>>(), Substitute.For<IRabbitWatcher>());
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_job_factory()
        {
            new DefaultMessageHandler(Substitute.For<IConsumerErrorHandler>(), null, Substitute.For<IRabbitWatcher>());
        }


        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Should_throw_exceeption_if_provide_null_rabbit_watcher()
        {
            new DefaultMessageHandler(Substitute.For<IConsumerErrorHandler>(), Substitute.For<Func<BasicDeliverEventArgs, Task>>(), null);
        }
    }
}
// ReSharper restore InconsistentNaming