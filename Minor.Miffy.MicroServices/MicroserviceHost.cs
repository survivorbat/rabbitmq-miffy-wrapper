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
        public IEnumerable<MicroserviceListener> Listeners { get; }

        /// <summary>
        /// Create a new Microservice host
        /// </summary>
        /// <param name="connection">IBusContext for the connection with the message bus</param>
        /// <param name="listeners">All the listeners</param>
        public MicroserviceHost(IBusContext<IConnection> connection, IEnumerable<MicroserviceListener> listeners)
        {
            Context = connection;
            Listeners = listeners;
        }

        /// <summary>
        /// Start listening for events
        /// </summary>
        public void Start()
        {
            foreach (var callback in Listeners)
            {
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
