using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RabbitMQ.Client;

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
        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            // Nothing to dispose of
        }

        /// <summary>
        /// The dictionary that will keep track of our data
        /// </summary>
        internal virtual Dictionary<TestBusKey, TestBusQueueWrapper<EventMessage>> DataQueues { get; } = 
            new Dictionary<TestBusKey, TestBusQueueWrapper<EventMessage>>();
        
        /// <summary>
        /// The dictionary that keeps track of running commands
        /// </summary>
        internal virtual Dictionary<string, TestBusQueueWrapper<CommandMessage>> CommandQueues { get; } = 
            new Dictionary<string, TestBusQueueWrapper<CommandMessage>>();

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

        /// <summary>
        /// Return a test command sender
        /// </summary>
        public ICommandSender CreateCommandSender() => new TestCommandSender(this);
        
        /// <summary>
        /// Return a test command sender
        /// </summary>
        public ICommandReceiver CreateCommandReceiver(string queueName) => new TestCommandReceiver(this, queueName);
    }
}