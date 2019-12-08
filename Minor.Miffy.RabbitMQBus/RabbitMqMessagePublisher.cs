using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Minor.Miffy.RabbitMQBus
{
    /// <summary>
    /// Low-level implementation of sending messages over a bus
    ///
    /// If you want to publish events, consider using the CommandPublisher from the microservices package.
    /// </summary>
    public class RabbitMqMessagePublisher : IMessageSender
    {
        /// <summary>
        /// Connection to the broker
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Name of the xchange being used
        /// </summary>
        private readonly string _exchangeName;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<RabbitMqMessagePublisher> _logger;

        /// <summary>
        /// Initialize a message publisher with a bus context
        /// </summary>
        public RabbitMqMessagePublisher(IBusContext<IConnection> context)
        {
            _connection = context.Connection;
            _exchangeName = context.ExchangeName;
            _logger = RabbitMqLoggerFactory.CreateInstance<RabbitMqMessagePublisher>();
        }

        /// <summary>
        /// Publish a message to the broker
        /// </summary>
        public void SendMessage(EventMessage message)
        {
            using IModel channel = _connection.CreateModel();

            _logger.LogInformation($"Publishing message with id {message.CorrelationId}" +
                                   $", topic {message.Topic} and type {message.EventType}");

            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Type = message.EventType;
            properties.Timestamp = new AmqpTimestamp(message.Timestamp);
            properties.CorrelationId = message.CorrelationId.ToString();

            channel.BasicPublish(_exchangeName, message.Topic, properties, message.Body);
        }
    }
}
