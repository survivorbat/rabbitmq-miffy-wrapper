using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Miffy.MicroServices
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost //: IDisposable
    {
        public MicroserviceHost(IBusContext<IConnection> connection)
        {
            // TODO
        }

        public void Start()
        {
            // TODO
        }

        // TODO
    }
}
