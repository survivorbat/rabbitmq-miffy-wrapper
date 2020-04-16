using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Miffy.TestBus
{
    public class TestMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// The test context being used
        /// </summary>
        public TestBusContext Context { get; }

        /// <summary>
        /// Indicate whether the receiver is paused or not
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// dDetermine if the receiver is listening
        /// </summary>
        protected bool IsListening { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger<TestMessageReceiver> Logger { get; }

        /// <summary>
        /// Create a new test receiver with a test context, queue name and expressions
        /// </summary>
        public TestMessageReceiver(TestBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            Context = context;
            QueueName = queueName;
            TopicFilters = topicExpressions;
            Logger = MiffyLoggerFactory.CreateInstance<TestMessageReceiver>();
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
                Logger.LogDebug($"Creating queue {QueueName} with topic {topic}");
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
                Logger.LogDebug($"Creating thread for queue {QueueName} with topic {topic}");

                Thread thread = new Thread(() => {
                    while (true)
                    {
                        // If the receiver is paused, keep it waiting
                        if (IsPaused)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        TestBusKey key = new TestBusKey(QueueName, topic);

                        Logger.LogTrace($"Waiting for message on queue {QueueName} with topic {topic}");

                        // Wait for messages
                        Context.DataQueues[key].AutoResetEvent.WaitOne();

                        // If the thread was paused while a message came in, reset the resetevent and continue
                        if (IsPaused)
                        {
                            Context.DataQueues[key].AutoResetEvent.Set();
                            continue;
                        }

                        Context.DataQueues[key].Queue.TryDequeue(out EventMessage result);

                        Logger.LogDebug($"Message received on queue {QueueName} with topic {topic}");

                        callback(result);
                    }
                });

                thread.IsBackground = true;
                thread.Start();
            }
        }

        /// <summary>
        /// Pause the receiver
        /// </summary>
        public void Pause()
        {
            if (!IsListening)
            {
                Logger.LogCritical($"Attempting to pause the MessageReceiver, but it is not even receiving messages yet with queue {QueueName}");
                throw new BusConfigurationException("Attempting to pause the TestMessageReceiver, but it is not even receiving messages.");
            }

            if (IsPaused)
            {
                Logger.LogCritical($"Attempting to pause the MessageReceiver, but it is already paused with queue {QueueName}");
                throw new BusConfigurationException("Attempting to pause the TestMessageReceiver, but it was already paused.");
            }

            Logger.LogInformation($"Pausing consumption of queue {QueueName}");

            IsPaused = true;
        }

        /// <summary>
        /// Resume the receiver
        /// </summary>
        public void Resume()
        {
            if (!IsListening)
            {
                Logger.LogCritical($"Attempting to resume the MessageReceiver, but it is not even receiving messages yet with queue {QueueName}");
                throw new BusConfigurationException("Attempting to resume the TestMessageReceiver, but it is not even receiving messages.");
            }

            if (!IsPaused)
            {
                Logger.LogCritical($"Attempting to resume the MessageReceiver, but it is not paused with queue {QueueName}");
                throw new BusConfigurationException("Attempting to resume the TestMessageReceiver, but it was not paused.");
            }

            Logger.LogInformation($"Resuming consumption of queue {QueueName}");

            IsPaused = false;
        }
    }
}
