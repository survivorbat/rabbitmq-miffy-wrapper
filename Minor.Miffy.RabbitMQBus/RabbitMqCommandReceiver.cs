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
        /// Is queue declared?
        /// </summary>
        private bool _queueDeclared;

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
            _model.QueueBind(QueueName, _context.ExchangeName, QueueName);
            _queueDeclared = true;
        }

        /// <summary>
        /// Start receiving commands and calling the callback
        /// </summary>
        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            if (!_queueDeclared)
            {
                throw new BusConfigurationException($"Queue {QueueName} has not been declared yet");    
            }
            
            _logger.LogTrace($"Start receiving commands on queue {QueueName}");

            var consumer = new EventingBasicConsumer(_model);
            
            consumer.Received += (model, ea) =>
            {
                _logger.LogInformation($"Received command on {QueueName} with reply queue {ea.BasicProperties.ReplyTo}");
                IBasicProperties replyProps = _model.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                CommandMessage request = new CommandMessage
                {
                    Body = ea.Body, 
                    Timestamp = ea.BasicProperties.Timestamp.UnixTime,
                    DestinationQueue = QueueName,
                    CorrelationId = Guid.Parse(ea.BasicProperties.CorrelationId)
                };
                CommandMessage response = callback(request);
                
                string responseMessage = JsonConvert.SerializeObject(response);
                
                _model.BasicPublish(
                    _context.ExchangeName, 
                    ea.BasicProperties.ReplyTo, 
                    replyProps, 
                    Encoding.Unicode.GetBytes(responseMessage));
                
                _model.BasicAck(ea.DeliveryTag, false);
            };
            
            _model.BasicConsume(consumer, QueueName);
        }

        /// <summary>
        /// Dispose of the created command
        /// </summary>
        public void Dispose() => _model.Dispose();
    }
}