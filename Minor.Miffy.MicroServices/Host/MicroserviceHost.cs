using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Host
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IDisposable
    {
        /// <summary>
        /// Connection context
        /// </summary>
        public readonly IBusContext<IConnection> Context;

        /// <summary>
        /// A list of queues that have a list of associated topics with handlers.
        /// </summary>
        public readonly IEnumerable<MicroserviceListener> Listeners;

        /// <summary>
        /// A list of queues that are used to receive commands
        /// </summary>
        public readonly IEnumerable<MicroserviceCommandListener> CommandListeners;

        /// <summary>
        /// List of message receivers
        /// </summary>
        protected readonly List<IMessageReceiver> MessageReceivers = new List<IMessageReceiver>();

        /// <summary>
        /// List of command receivers
        /// </summary>
        protected readonly List<ICommandReceiver> CommandReceivers = new List<ICommandReceiver>();

        /// <summary>
        /// A logger factory to log the start
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Create a new Microservice host
        /// </summary>
        /// <param name="connection">IBusContext for the connection with the message bus</param>
        /// <param name="listeners">All the listeners</param>
        /// <param name="commandListeners">All the command listeners</param>
        /// <param name="logger">Logging instance</param>
        public MicroserviceHost(
            IBusContext<IConnection> connection,
            IEnumerable<MicroserviceListener> listeners,
            IEnumerable<MicroserviceCommandListener> commandListeners,
            ILogger<MicroserviceHost> logger)
        {
            Context = connection;
            Listeners = listeners;
            CommandListeners = commandListeners;
            Logger = logger;
        }

        /// <summary>
        /// Start listening for events
        /// </summary>
        public virtual void Start()
        {
            foreach (MicroserviceListener callback in Listeners)
            {
                Logger.LogInformation($"Registering queue {callback.Queue} with expressions {string.Join(", ", callback.TopicExpressions)}");

                IMessageReceiver receiver = Context.CreateMessageReceiver(callback.Queue, callback.TopicExpressions);
                receiver.StartReceivingMessages();
                receiver.StartHandlingMessages(callback.Callback);
                MessageReceivers.Add(receiver);
            }

            foreach (MicroserviceCommandListener callback in CommandListeners)
            {
                Logger.LogInformation($"Registering command queue {callback.Queue}");

                ICommandReceiver receiver = Context.CreateCommandReceiver(callback.Queue);
                receiver.DeclareCommandQueue();
                receiver.StartReceivingCommands(callback.Callback);
                CommandReceivers.Add(receiver);
            }
        }

        /// <summary>
        /// Dispose of the context
        /// </summary>
        public virtual void Dispose()
        {
            MessageReceivers.ForEach(e => e.Dispose());
            CommandReceivers.ForEach(e => e.Dispose());
            Context.Dispose();
        }
    }
}
