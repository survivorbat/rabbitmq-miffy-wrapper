using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqCommandReceiver : ICommandReceiver
    {
        /// <summary>
        /// Testbus context
        /// </summary>
        private readonly IBusContext<IConnection> _context;
        
        /// <summary>
        /// Model used to send messages
        /// </summary>
        private readonly IModel _model;
        
        /// <summary>
        /// Name of the queue for th ecommands
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Initialize a receiver with a context and queue name
        /// </summary>
        public RabbitMqCommandReceiver(IBusContext<IConnection> context)
        {
            _context = context;
            _model = context.Connection.CreateModel();
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
            var consumer = new EventingBasicConsumer(_model);
            consumer.Received += (sender, args) => Console.WriteLine("yay");
            _model.BasicConsume(consumer, QueueName);
        }

        /// <summary>
        /// Dispose of the created command
        /// </summary>
        public void Dispose() => _model.Dispose();
    }
}