using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Miffy.MicroServices
{
    /// <summary>
    /// This attribute should decorate each event listening class.
    /// The queueName is the name of the RabbitMQ-queue on which it 
    /// will listen to incoming events.
    /// A MicroserviceHost cannot have two EventListeners that 
    /// listen to the same queue name.
    /// </summary>
    public class EventListenerAttribute : Attribute
    {
        public string QueueName { get; }
        
        public EventListenerAttribute(string queueName) => QueueName = queueName;

        // TODO
    }
}
