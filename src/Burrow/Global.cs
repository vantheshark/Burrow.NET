using System;
using System.Threading.Tasks;
using Burrow.Internal;

namespace Burrow
{
    public static class Global
    {
        private static readonly DefaultTaskCreationOptionProvider _defaultTaskCreationOptionProvider = new DefaultTaskCreationOptionProvider();

        public static ISerializer DefaultSerializer = new JavaScriptSerializer();
        public static IRabbitWatcher DefaultWatcher = new ConsoleWatcher();
        public static ICorrelationIdGenerator DefaultCorrelationIdGenerator = new SimpleCorrelationIdGenerator();
        public static ITypeNameSerializer DefaultTypeNameSerializer = new TypeNameSerializer();
        
        /// <summary>
        /// When the MessageHandler handles the message using the callback, it will create a Task (TPL) for it.
        /// So this instance will help the MessageHandler to determine which type of Thread to create, either from Threadpool or a dedicated Thread
        /// The default value of this is an implementation that based on the available amount of worker threads in Threadpool to return the proper TaskCreationOption
        /// <para>In Win32, the MaxWorker is 1023 while the number is quite high in Win64 bit. So if your app is running as 64 bits, you probably need to call ThreadPool.SetMaxThreads()
        /// to a proper number or simply change the Global.DefaultTaskCreationOptionsProvider func to return TaskCreationOptions.LongRunning</para>
        /// <para>The reason for this complexity is sometime your Task/ThreadPool is queued but cannot be started, probably the main application itself was creating so
        /// many threads, or the ThreadPool is completly full or many workers have been queued and that problem can block everything</para>
        /// </summary>
        public static Func<TaskCreationOptions> DefaultTaskCreationOptionsProvider = () => _defaultTaskCreationOptionProvider.GetOptions();
        
        /// <summary>
        /// The higher the number is, the more threads a tunnel will create to consume messages in the queue.
        /// If set to 1, it means the messages will be consumed sequently
        /// This value is used by the TunnelFactory when it create a RabbitTunnel
        /// This value is NOLONGER used to call IModel.BasicQos, if you want to do so, use PreFetchSize instead
        /// </summary>
        public static ushort DefaultConsumerBatchSize = 4;

        /// This value is also used to call IModel.BasicQos, which eventually sets the maximum amount of messages stay on the Unacknowledged list when they are consumed
        /// <para>If you app decides not to ack the message immediately but just queueing everything received from Burrow.NET and ack later once they're finished, you will not get morethan 
        /// this number of messages in your internal queue because RabbitMQ.Client basically has a waithanler to block the consuming thread, only this number of messages can be dequeued.
        /// If this is a potential problem, you have to ack atleast a message to receive a new one from RabbitMQ</para>
        public static ushort PreFetchSize = 128;
        
        /// <summary>
        /// Set to true will save the message to disk when it's published, default is true.
        /// If its value is false, the messages will be removed when the server is restarted.
        /// This value is used by the default TunnelFactory when it create a RabbitTunnel
        /// </summary>
        public static bool DefaultPersistentMode = true;

        public static string DefaultErrorQueueName = "Burrow.Queue.Error";
        public static string DefaultErrorExchangeName = "Burrow.Exchange.Error";


        static Global()
        {
            TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;
        }

        static void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            foreach (var ex in e.Exception.InnerExceptions)
            {
                // NOTE: Should not observe the msg here, let the client of this library deal with that since there could be 
                // other TPL Task created by the developers
                
                DefaultWatcher.Error(ex);
            }
        }

        internal static Func<TaskContinuationOptions> DefaultTaskContinuationOptionsProvider = () => (DefaultTaskCreationOptionsProvider() & TaskCreationOptions.LongRunning) > 0 
                                                                                             ? TaskContinuationOptions.LongRunning 
                                                                                             : TaskContinuationOptions.PreferFairness;
    }
}
