using System;

namespace Miffy.MicroServices.Commands
{
    /// <summary>
    /// This attribute should decorate each event listening class.
    /// The queueName is the name of the RabbitMQ-queue on which it
    /// will listen to incoming events.
    /// A MicroserviceHost cannot have two EventListeners that
    /// listen to the same queue name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandListenerAttribute : Attribute
    {
        public string QueueName { get; }

        public CommandListenerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}
