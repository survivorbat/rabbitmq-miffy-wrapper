using RabbitMQ.Client;

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
            
        }

        /// <summary>
        /// Start receiving commands and calling the callback
        /// </summary>
        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            
        }

        /// <summary>
        /// Empty dispose since there is nothing to dispose of
        /// </summary>
        public void Dispose() { }
    }
}