using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Miffy.MicroServices
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IDisposable
    {
        public IBusContext<IConnection> Context { get; }
        
        public MicroserviceHost(IBusContext<IConnection> connection) => Context = connection;

        public void Start()
        {
            
        }

        public void Dispose() => Context.Dispose();
    }
}
