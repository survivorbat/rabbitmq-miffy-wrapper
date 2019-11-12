using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqMessagePublisher : IMessageSender
    {
        private readonly IConnection _connection;
        private readonly string _exchangeName;

        public RabbitMqMessagePublisher(IBusContext<IConnection> context)
        {
            _connection = context.Connection;
            _exchangeName = context.ExchangeName;
        }

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