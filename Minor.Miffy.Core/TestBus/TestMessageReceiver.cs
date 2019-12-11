using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Minor.Miffy.TestBus
{
    public class TestMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// The test context being used
        /// </summary>
        public TestBusContext Context { get; protected set; }

        /// <summary>
        /// dDetermine if the receiver is listening
        /// </summary>
        protected bool IsListening { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger<TestMessageReceiver> _logger;

        /// <summary>
        /// Create a new test receiver with a test context, queue name and expressions
        /// </summary>
        public TestMessageReceiver(TestBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            Context = context;
            QueueName = queueName;
            TopicFilters = topicExpressions;
            _logger = MiffyLoggerFactory.CreateInstance<TestMessageReceiver>();
        }

        /// <summary>
        /// Empty dispose since we don't have anything to dispose of
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual void Dispose()
        {
            // Nothing to dispose of
        }

        /// <summary>
        /// Name of the queue
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Topic filters to filter the listener on
        /// </summary>
        public IEnumerable<string> TopicFilters { get; }

        /// <summary>
        /// Create a new queue wrapper object in the dictionary of the bus
        /// </summary>
        public virtual void StartReceivingMessages()
        {
            if (IsListening)
            {
                throw new BusConfigurationException("Receiver is already listening to events!");
            }

            foreach (string topic in TopicFilters)
            {
                _logger.LogDebug($"Creating queue {QueueName} with topic {topic}");
                TestBusKey key = new TestBusKey(QueueName, topic);
                Context.DataQueues[key] = new TestBusQueueWrapper<EventMessage>();
            }

            IsListening = true;
        }

        /// <summary>
        /// Create a new thread with an endless loop that waits for new
        /// messages in a specific queue
        /// </summary>
        public virtual void StartHandlingMessages(EventMessageReceivedCallback callback)
        {
            foreach (string topic in TopicFilters)
            {
                _logger.LogDebug($"Creating thread for queue {QueueName} with topic {topic}");

                Thread thread = new Thread(() => {
                    while (true)
                    {
                        TestBusKey key = new TestBusKey(QueueName, topic);

                        _logger.LogTrace($"Waiting for message on queue {QueueName} with topic {topic}");

                        Context.DataQueues[key].AutoResetEvent.WaitOne();
                        Context.DataQueues[key].Queue.TryDequeue(out EventMessage result);

                        _logger.LogDebug($"Message received on queue {QueueName} with topic {topic}");

                        callback(result);
                    }
                });

                thread.IsBackground = true;
                thread.Start();
            }
        }
    }
}
