using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Minor.Miffy.MicroServices
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
        private readonly IEnumerable<MicroserviceListener> _listeners;

        /// <summary>
        /// A logger factory to log the start
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Create a new Microservice host
        /// </summary>
        /// <param name="connection">IBusContext for the connection with the message bus</param>
        /// <param name="listeners">All the listeners</param>
        /// <param name="logger">Logging instance</param>
        public MicroserviceHost(IBusContext<IConnection> connection, IEnumerable<MicroserviceListener> listeners, ILogger<MicroserviceHost> logger)
        {
            Context = connection;
            _listeners = listeners;
            _logger = logger;
        }

        /// <summary>
        /// Start listening for events
        /// </summary>
        public void Start()
        {
            foreach (var callback in _listeners)
            {
                _logger.LogInformation($"Registering queue {callback.Queue} with expressions {string.Join(", ", callback.TopicExpressions)}");
                
                var receiver = Context.CreateMessageReceiver(callback.Queue, callback.TopicExpressions);
                receiver.StartReceivingMessages();
                receiver.StartHandlingMessages(callback.Callback);
            }
        }

        /// <summary>
        /// Dispose of the context
        /// </summary>
        public void Dispose() => Context.Dispose();
    }
}
