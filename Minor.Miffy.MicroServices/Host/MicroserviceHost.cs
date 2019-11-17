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
        public IBusContext<IConnection> Context { get; }

        /// <summary>
        /// A list of queues that have a list of associated topics with handlers.
        /// </summary>
        internal readonly IEnumerable<MicroserviceListener> Listeners;

        /// <summary>
        /// A list of queues that are used to receive commands
        /// </summary>
        internal readonly IEnumerable<MicroserviceCommandListener> CommandListeners;

        /// <summary>
        /// List of message receivers
        /// </summary>
        private readonly List<IMessageReceiver> _messageReceivers = new List<IMessageReceiver>();
        
        /// <summary>
        /// List of command receivers
        /// </summary>
        private readonly List<ICommandReceiver> _commandReceivers = new List<ICommandReceiver>();

        /// <summary>
        /// A logger factory to log the start
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Create a new Microservice host
        /// </summary>
        /// <param name="connection">IBusContext for the connection with the message bus</param>
        /// <param name="listeners">All the listeners</param>
        /// <param name="commandListeners">All the command listeners</param>
        /// <param name="logger">Logging instance</param>
        public MicroserviceHost(IBusContext<
            IConnection> connection, 
            IEnumerable<MicroserviceListener> listeners, 
            IEnumerable<MicroserviceCommandListener> commandListeners,
            ILogger<MicroserviceHost> logger)
        {
            Context = connection;
            Listeners = listeners;
            CommandListeners = commandListeners;
            _logger = logger;
        }

        /// <summary>
        /// Start listening for events
        /// </summary>
        public void Start()
        {
            foreach (var callback in Listeners)
            {
                _logger.LogInformation($"Registering queue {callback.Queue} with expressions {string.Join(", ", callback.TopicExpressions)}");
                
                var receiver = Context.CreateMessageReceiver(callback.Queue, callback.TopicExpressions);
                receiver.StartReceivingMessages();
                receiver.StartHandlingMessages(callback.Callback);
                _messageReceivers.Add(receiver);
            }

            foreach (var callback in CommandListeners)
            {
                _logger.LogInformation($"Registering command queue {callback.Queue}");

                var receiver = Context.CreateCommandReceiver(callback.Queue);
                receiver.DeclareCommandQueue();
                receiver.StartReceivingCommands(callback.Callback);
                _commandReceivers.Add(receiver);
            }
        }

        /// <summary>
        /// Dispose of the context
        /// </summary>
        public void Dispose()
        {
            _messageReceivers.ForEach(e => e.Dispose());
            _commandReceivers.ForEach(e => e.Dispose());
            Context.Dispose();
        }
    }
}
