using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Miffy.RabbitMQBus
{
    /// <summary>
    /// Low-level implementation of receiving commands from a bus
    ///
    /// If you want to receive commands in a listener, consider using Commandlisteners in the microservice package
    /// </summary>
    public class RabbitMqCommandReceiver : ICommandReceiver
    {
        private const string CommandErrorType = "CommandError";

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
        /// Name of the queue for the commands
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
            if (_queueDeclared)
            {
                throw new BusConfigurationException($"Queue {QueueName} has already been declared!");
            }

            _logger.LogTrace($"Declaring command queue {QueueName}");
            _model.QueueDeclare(QueueName, true, false, false, null);
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

            EventingBasicConsumer consumer = new EventingBasicConsumer(_model);

            consumer.Received += (model, ea) =>
            {
                _logger.LogInformation(
                    $"Received command on {QueueName} with reply queue {ea.BasicProperties.ReplyTo}");
                IBasicProperties replyProps = _model.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                CommandMessage request = new CommandMessage
                {
                    Body = ea.Body,
                    Timestamp = ea.BasicProperties.Timestamp.UnixTime,
                    DestinationQueue = QueueName,
                    EventType = ea.BasicProperties.Type,
                    ReplyQueue = ea.BasicProperties.ReplyTo,
                    CorrelationId = Guid.Parse(ea.BasicProperties.CorrelationId)
                };

                CommandMessage response;
                try
                {
                    response = callback(request);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error occured while handling command, command id {ea.BasicProperties.CorrelationId} " +
                                     $"with exception {e.Message}");

                    response = new CommandError
                    {
                        Exception = e.InnerException ?? e,
                        EventType = CommandErrorType
                    };
                }

                string responseMessage = JsonConvert.SerializeObject(response);

                _logger.LogTrace($"Publishing command response with id {ea.BasicProperties.CorrelationId} " +
                                 $"reply queue {ea.BasicProperties.ReplyTo} and body {responseMessage}");

                _model.BasicPublish(
                    "",
                    ea.BasicProperties.ReplyTo,
                    replyProps,
                    Encoding.Unicode.GetBytes(responseMessage));

                _model.BasicAck(ea.DeliveryTag, false);
            };

            _model.BasicConsume(QueueName, false, "", false, false, null, consumer);
        }

        /// <summary>
        /// Dispose of the created command
        /// </summary>
        public void Dispose() => _model.Dispose();
    }
}
