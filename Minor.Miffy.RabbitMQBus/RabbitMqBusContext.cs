using System.Collections.Generic;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqBusContext : IBusContext<IConnection>
    {
        public void Dispose() => Connection.Dispose();

        public IConnection Connection { get; }
        public string ExchangeName { get; }

        public RabbitMqBusContext(IConnection connection, string exchangeName)
        {
            Connection = connection;
            ExchangeName = exchangeName;
        }
        
        public IMessageSender CreateMessageSender() => new RabbitMqMessagePublisher(this);

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions) => 
            new RabbitMqMessageReceiver(this, queueName, topicExpressions);

        public ICommandSender CreateCommandSender() => new RabbitMqCommandSender(this);

        public ICommandReceiver CreateCommandReceiver() => new RabbitMqCommandReceiver(this);
    }
}