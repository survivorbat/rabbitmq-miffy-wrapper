using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Minor.Miffy.TestBus
{
    public class TestMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// The test context being used
        /// </summary>
        public readonly TestBusContext Context;

        /// <summary>
        /// dDetermine if the receiver is listening
        /// </summary>
        private bool _isListening;

        /// <summary>
        /// Logger
        /// </summary>
        private ILogger<TestMessageReceiver> _logger;

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
        public void Dispose() { }

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
        public void StartReceivingMessages()
        {
            if (_isListening) throw new BusConfigurationException("Receiver is already listening to events!");

            foreach (string topic in TopicFilters)
            {
                _logger.LogDebug($"Creating queue {QueueName} with topic {topic}");
                TestBusKey key = new TestBusKey(QueueName, topic);
                Context.DataQueues[key] = new TestBusQueueWrapper<EventMessage>();
            }

            _isListening = true;
        }

        /// <summary>
        /// Create a new thread with an endless loop that waits for new
        /// messages in a specific queue
        /// </summary>
        public void StartHandlingMessages(EventMessageReceivedCallback callback)
        {
            foreach (string topic in TopicFilters)
            {
                _logger.LogDebug($"Creating thread for queue {QueueName} with topic {topic}");
                
                var thread = new Thread(() => {
                    while (true)
                    {
                        TestBusKey key = new TestBusKey(QueueName, topic);

                        _logger.LogTrace($"Waiting for message on queue {QueueName} with topic {topic}");

                        Context.DataQueues[key].AutoResetEvent.WaitOne();
                        Context.DataQueues[key].Queue.TryDequeue(out var result);
                        
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