using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
        public Dictionary<(string, string[]), EventMessageReceivedCallback> Listeners { get; }

        /// <summary>
        /// Create a new Microservice host
        /// </summary>
        /// <param name="connection">IBusContext for the connection with the message bus</param>
        /// <param name="listeners">All the listeners</param>
        public MicroserviceHost(IBusContext<IConnection> connection, Dictionary<(string, string[]), EventMessageReceivedCallback> listeners)
        {
            Context = connection;
            Listeners = listeners;
        }

        /// <summary>
        /// Start listening for events
        /// </summary>
        public void Start()
        {
            foreach (var (queue, topics) in Listeners.Keys)
            {
                var receiver = Context.CreateMessageReceiver(queue, topics);
                receiver.StartReceivingMessages();
                receiver.StartHandlingMessages(Listeners[(queue, topics)]);
            }
        }

        /// <summary>
        /// Dispose of the context
        /// </summary>
        public void Dispose() => Context.Dispose();
    }
}
