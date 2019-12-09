using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Minor.Miffy.TestBus
{
    public class TestCommandReceiver : ICommandReceiver
    {
        /// <summary>
        /// Testbus context
        /// </summary>
        private readonly TestBusContext _context;

        /// <summary>
        /// Name of the queue for th ecommands
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Whether the queue is already declared or not
        /// </summary>
        private bool _queueDeclared;

        /// <summary>
        /// Initialize a receiver with a context and queue name
        /// </summary>
        public TestCommandReceiver(TestBusContext context, string queueName)
        {
            _context = context;
            QueueName = queueName;
        }

        /// <summary>
        /// Declare the queue with the given queue name
        /// </summary>
        public void DeclareCommandQueue()
        {
            _context.CommandQueues[QueueName] = new TestBusQueueWrapper<CommandMessage>();
            _queueDeclared = true;
        }

        /// <summary>
        /// Start receiving commands and calling the callback
        /// </summary>
        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            if (!_queueDeclared)
            {
                throw new BusConfigurationException($"Queue {QueueName} has not been declared yet.");
            }

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    _context.CommandQueues[QueueName].AutoResetEvent.WaitOne();
                    _context.CommandQueues[QueueName].Queue.TryDequeue(out CommandMessage input);
                    CommandMessage result = callback(input);
                    _context.CommandQueues[input.ReplyQueue].Queue.Enqueue(result);
                    _context.CommandQueues[input.ReplyQueue].AutoResetEvent.Set();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Empty dispose since there is nothing to dispose of
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            // Nothing to dispose of
        }
    }
}
