using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Minor.Miffy.TestBus
{
    /// <summary>
    /// A test context that allows for an in-memory RabbitMQ queue
    /// using a queue and a separate thread.
    /// </summary>
    public class TestBusContext : IBusContext<IConnection>
    {
        /// <summary>
        /// Unimplemented since we don't use a connection
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// The dictionary that will keep track of our data
        /// </summary>
        internal virtual Dictionary<TestBusKey, TestBusQueueWrapper> DataQueues { get; } = new Dictionary<TestBusKey, TestBusQueueWrapper>();

        /// <summary>
        /// Properties that do not matter for the queue but are
        /// here to comply with the interface
        /// </summary>
        public IConnection Connection => throw new NotImplementedException();
        public string ExchangeName => throw new NotImplementedException();

        /// <summary>
        /// Return a test message sender to use for testing purposes
        /// </summary>
        public IMessageSender CreateMessageSender() => new TestMessageSender(this);

        /// <summary>
        /// Return a test message receiver to use for testing purposes
        /// </summary>
        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions) => 
            new TestMessageReceiver(this, queueName, topicExpressions);

        public ICommandSender CreateCommandSender() => new TestCommandSender(this);
        public ICommandReceiver CreateCommandReceiver(string queueName) => new TestCommandReceiver(this, queueName);
    }
}