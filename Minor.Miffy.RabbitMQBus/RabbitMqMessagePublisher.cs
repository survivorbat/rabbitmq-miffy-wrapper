using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

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
        protected readonly IConnection Connection;

        /// <summary>
        /// Name of the xchange being used
        /// </summary>
        protected readonly string ExchangeName;

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger<RabbitMqMessagePublisher> Logger;

        /// <summary>
        /// Initialize a message publisher with a bus context
        /// </summary>
        public RabbitMqMessagePublisher(IBusContext<IConnection> context)
        {
            Connection = context.Connection;
            ExchangeName = context.ExchangeName;
            Logger = RabbitMqLoggerFactory.CreateInstance<RabbitMqMessagePublisher>();
        }

        /// <summary>
        /// Publish a message to the broker
        /// </summary>
        public virtual void SendMessage(EventMessage message)
        {
            using IModel channel = Connection.CreateModel();

            Logger.LogInformation($"Publishing message with id {message.CorrelationId}" +
                                   $", topic {message.Topic} and type {message.EventType}");

            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Type = message.EventType;
            properties.Timestamp = new AmqpTimestamp(message.Timestamp);
            properties.CorrelationId = message.CorrelationId.ToString();

            channel.BasicPublish(ExchangeName, message.Topic, properties, message.Body);
        }
    }
}
