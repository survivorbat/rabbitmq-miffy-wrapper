using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Minor.Miffy.TestBus
{
    public class TestCommandReceiver : ICommandReceiver
    {
        /// <summary>
        /// Testbus context
        /// </summary>
        protected readonly TestBusContext Context;

        /// <summary>
        /// Name of the queue for the commands
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Whether the queue is already declared or not
        /// </summary>
        protected bool QueueDeclared;

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger<TestCommandReceiver> Logger;

        /// <summary>
        /// Initialize a receiver with a context and queue name
        /// </summary>
        public TestCommandReceiver(TestBusContext context, string queueName)
        {
            Context = context;
            QueueName = queueName;
            Logger = MiffyLoggerFactory.CreateInstance<TestCommandReceiver>();
        }

        public bool IsPaused { get; protected set; }

        /// <summary>
        /// Declare the queue with the given queue name
        /// </summary>
        public virtual void DeclareCommandQueue()
        {
            Context.CommandQueues[QueueName] = new TestBusQueueWrapper<CommandMessage>();
            QueueDeclared = true;
        }

        /// <summary>
        /// Start receiving commands and calling the callback
        /// </summary>
        public virtual void StartReceivingCommands(CommandReceivedCallback callback)
        {
            if (!QueueDeclared)
            {
                throw new BusConfigurationException($"Queue {QueueName} has not been declared yet.");
            }

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    // If the receiver is paused, keep it waiting
                    if (IsPaused)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    Context.CommandQueues[QueueName].AutoResetEvent.WaitOne();

                    // If the thread was paused while a message came in, reset the resetevent and continue
                    if (IsPaused)
                    {
                        Context.CommandQueues[QueueName].AutoResetEvent.Set();
                        continue;
                    }

                    Context.CommandQueues[QueueName].Queue.TryDequeue(out CommandMessage input);
                    CommandMessage result = callback(input);
                    Context.CommandQueues[input.ReplyQueue].Queue.Enqueue(result);
                    Context.CommandQueues[input.ReplyQueue].AutoResetEvent.Set();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Pause the command receiver
        /// </summary>
        public void Pause()
        {
            if (IsPaused)
            {
                Logger.LogCritical($"Attempting to pause the CommandReceiver, but it is already paused with queue {QueueName}");
                throw new BusConfigurationException("Attempting to pause the TestCommandReceiver, but it was already paused.");
            }

            Logger.LogInformation($"Pausing consumption of queue {QueueName}");

            IsPaused = true;
        }

        /// <summary>
        /// Resume the command receiver
        /// </summary>
        public void Resume()
        {
            if (!IsPaused)
            {
                Logger.LogCritical($"Attempting to resume the CommandReceiver, but it is not paused with queue {QueueName}");
                throw new BusConfigurationException("Attempting to resume the TestCommandReceiver, but it was not paused.");
            }

            Logger.LogInformation($"Resuming consumption of queue {QueueName}");

            IsPaused = false;
        }

        /// <summary>
        /// Empty dispose since there is nothing to dispose of
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual void Dispose()
        {
            // Nothing to dispose of
        }
    }
}
