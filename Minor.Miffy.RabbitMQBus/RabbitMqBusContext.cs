using System.Collections.Generic;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqBusContext : IBusContext<IConnection>
    {
        /// <summary>
        /// Dispose of the connection
        /// </summary>
        public void Dispose() => Connection.Dispose();

        /// <summary>
        /// Connection to the broker
        /// </summary>
        public IConnection Connection { get; }
        
        /// <summary>
        /// Name of the exhangeb eing used
        /// </summary>
        public string ExchangeName { get; }

        /// <summary>
        /// Initialize an opened connection with a certain exchange name
        /// </summary>
        public RabbitMqBusContext(IConnection connection, string exchangeName)
        {
            Connection = connection;
            ExchangeName = exchangeName;
        }
        
        /// <summary>
        /// Create a message publisher with the current context
        /// </summary>
        public IMessageSender CreateMessageSender() => new RabbitMqMessagePublisher(this);

        /// <summary>
        /// Create a message receiver with the current context, queue name and all topic expressions
        /// </summary>
        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions) => 
            new RabbitMqMessageReceiver(this, queueName, topicExpressions);
        
        /// <summary>
        /// Create a command sender
        /// </summary>
        public ICommandSender CreateCommandSender() => new RabbitMqCommandSender(this);

        /// <summary>
        /// Create a receiver for commands with a stringname
        /// </summary>
        public ICommandReceiver CreateCommandReceiver(string queueName) => new RabbitMqCommandReceiver(this, queueName);
    }
}