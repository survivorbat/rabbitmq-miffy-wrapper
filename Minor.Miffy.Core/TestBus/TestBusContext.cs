using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
        public readonly Dictionary<TestBusKey, TestBusQueueWrapper> DataQueues = new Dictionary<TestBusKey, TestBusQueueWrapper>();
        
        /// <summary>
        /// Properties that do not matter for the queue but are
        /// here to comply with the interface
        /// </summary>
        public IConnection Connection { get; set; }
        public string ExchangeName { get; set; }

        /// <summary>
        /// Return a test message sender to use for testing purposes
        /// </summary>
        public IMessageSender CreateMessageSender() => new TestMessageSender(this);

        /// <summary>
        /// Return a test message receiver to use for testing purposes
        /// </summary>
        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions) => 
            new TestMessageReceiver(this, queueName, topicExpressions);
    }

    /// <summary>
    /// Wrapper class that contains a reset event, a queue and a topicname
    /// </summary>
    public class TestBusQueueWrapper
    {
        /// <summary>
        /// Reset event to wait for
        /// </summary>
        public AutoResetEvent AutoResetEvent { get; } = new AutoResetEvent(false);
        
        /// <summary>
        /// The actual queue with messages
        /// </summary>
        public Queue<EventMessage> Queue { get; } = new Queue<EventMessage>();
    }

    /// <summary>
    /// Wrapper class that contains a queuename and a topic name
    /// </summary>
    public class TestBusKey
    {
        /// <summary>
        /// Name of the queue
        /// </summary>
        public string QueueName { get; set; }
        
        /// <summary>
        /// Name of the topic
        /// </summary>
        public string TopicName { get; set; }
    }
}