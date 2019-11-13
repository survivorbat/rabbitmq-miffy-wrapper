using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Minor.Miffy.RabbitMQBus
{
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
        /// Initialize a message publisher with a bus context
        /// </summary>
        public RabbitMqMessagePublisher(IBusContext<IConnection> context)
        {
            _connection = context.Connection;
            _exchangeName = context.ExchangeName;
        }

        /// <summary>
        /// Publish a message to the broker
        /// </summary>
        public void SendMessage(EventMessage message)
        {
            using var channel = _connection.CreateModel();
            
            IBasicProperties basicProperties = new BasicProperties
            {
                Type = message.EventType,
                Timestamp = new AmqpTimestamp(message.Timestamp),
                CorrelationId = message.CorrelationId.ToString(),
            };
                
            channel.BasicPublish(_exchangeName, message.Topic, basicProperties, message.Body);
        }
    }
}