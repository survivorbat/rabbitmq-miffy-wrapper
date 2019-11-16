using System;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        /// Logger
        /// </summary>
        private readonly ILogger<RabbitMqCommandReceiver> _logger;

        /// <summary>
        /// Name of the queue for th ecommands
        /// </summary>
        public string QueueName { get; }
        
        /// <summary>
        /// Initialize a receiver with a context and queue name
        /// </summary>
        public RabbitMqCommandReceiver(IBusContext<IConnection> context, string queueName)
        {
            _context = context;
            _model = context.Connection.CreateModel();
            QueueName = queueName;
            _logger = RabbitMqLoggerFactory.CreateInstance<RabbitMqCommandReceiver>();
        }

        /// <summary>
        /// Declare the queue with the given queue name
        /// </summary>
        public void DeclareCommandQueue()
        {
            _logger.LogTrace($"Declaring command queue {QueueName}");
            _model.QueueDeclare(QueueName, true, false, false);
            _model.BasicQos(0, 1, false);
        }

        /// <summary>
        /// Start receiving commands and calling the callback
        /// </summary>
        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            _logger.LogTrace($"Start receiving commands on queue {QueueName}");

            var consumer = new EventingBasicConsumer(_model);
            
            consumer.Received += (model, ea) =>
            {
                IBasicProperties replyProps = _model.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                string messageString = Encoding.Unicode.GetString(ea.Body); 
                CommandMessage request = JsonConvert.DeserializeObject<CommandMessage>(messageString);
                CommandMessage response = callback(request);
                
                string responseMessage = JsonConvert.SerializeObject(response);
                byte[] responseBody = Encoding.Unicode.GetBytes(responseMessage);
                _model.BasicPublish(_context.ExchangeName, ea.BasicProperties.ReplyTo, replyProps, responseBody);
            };
            
            _model.BasicConsume(QueueName, true, consumer);
        }

        /// <summary>
        /// Dispose of the created command
        /// </summary>
        public void Dispose() => _model.Dispose();
    }
}