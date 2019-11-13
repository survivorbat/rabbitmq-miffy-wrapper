using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Minor.Miffy.TestBus
{
    public class TestMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// The test context being used
        /// </summary>
        public readonly TestBusContext Context;

        /// <summary>
        /// Create a new test receiver with a test context, queue name and expressions
        /// </summary>
        public TestMessageReceiver(TestBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            Context = context;
            QueueName = queueName;
            TopicFilters = topicExpressions;
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
            foreach (string topic in TopicFilters)
            {
                TestBusKey key = new TestBusKey {TopicName = topic, QueueName = QueueName};
                Context.DataQueues[key] = new TestBusQueueWrapper();
            }
        }

        /// <summary>
        /// Create a new thread with an endless loop that waits for new
        /// messages in a specific queue
        /// </summary>
        public void StartHandlingMessages(EventMessageReceivedCallback callback)
        {
            var thread = new Thread(() => {
                while (true)
                {
                    foreach (string topic in TopicFilters)
                    {
                        TestBusKey key = new TestBusKey {TopicName = topic, QueueName = QueueName};
                        TestBusQueueWrapper wrapper = Context.DataQueues[key];

                        if (wrapper == null) continue;
                        
                        wrapper.AutoResetEvent.WaitOne(1000);
                        
                        callback(wrapper.Queue.Dequeue());
                    }
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }
    }
}