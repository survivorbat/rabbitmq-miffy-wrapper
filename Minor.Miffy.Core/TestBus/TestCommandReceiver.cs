using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Minor.Miffy.TestBus
{
    public class TestCommandReceiver : ICommandReceiver
    {
        /// <summary>
        /// Testbus context
        /// </summary>
        protected readonly TestBusContext Context;

        /// <summary>
        /// Name of the queue for th ecommands
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Whether the queue is already declared or not
        /// </summary>
        protected bool QueueDeclared;

        /// <summary>
        /// Initialize a receiver with a context and queue name
        /// </summary>
        public TestCommandReceiver(TestBusContext context, string queueName)
        {
            Context = context;
            QueueName = queueName;
        }

        /// <summary>
        /// Declare the queue with the given queue name
        /// </summary>
        public virtual void DeclareCommandQueue()
        {
            Context.CommandQueues[QueueName] = new TestBusQueueWrapper<CommandMessage>();
            QueueDeclared = true;
        }

        /// <summary>
        /// Start receiving commands and calling the callback
        /// </summary>
        public virtual void StartReceivingCommands(CommandReceivedCallback callback)
        {
            if (!QueueDeclared)
            {
                throw new BusConfigurationException($"Queue {QueueName} has not been declared yet.");
            }

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    Context.CommandQueues[QueueName].AutoResetEvent.WaitOne();
                    Context.CommandQueues[QueueName].Queue.TryDequeue(out CommandMessage input);
                    CommandMessage result = callback(input);
                    Context.CommandQueues[input.ReplyQueue].Queue.Enqueue(result);
                    Context.CommandQueues[input.ReplyQueue].AutoResetEvent.Set();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Empty dispose since there is nothing to dispose of
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual void Dispose()
        {
            // Nothing to dispose of
        }
    }
}
