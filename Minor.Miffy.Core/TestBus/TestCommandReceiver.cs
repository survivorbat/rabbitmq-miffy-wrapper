using RabbitMQ.Client;

namespace Minor.Miffy.TestBus
{
    public class TestCommandReceiver : ICommandReceiver
    {
        /// <summary>
        /// Testbus context
        /// </summary>
        private readonly TestBusContext _context;
        private readonly IModel _model;
        
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
            _model = context.Connection.CreateModel();
            QueueName = queueName;
        }
        
        /// <summary>
        /// Declare the queue with the given queue name
        /// </summary>
        public void DeclareCommandQueue() => _model.QueueDeclare(QueueName);

        /// <summary>
        /// Start receiving commands and calling the callback
        /// </summary>
        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            
        }

        /// <summary>
        /// Dispose of the created command
        /// </summary>
        public void Dispose() => _model.Dispose();
    }
}