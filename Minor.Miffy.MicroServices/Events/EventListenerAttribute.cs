using System;

namespace Minor.Miffy.MicroServices.Events
{
    /// <summary>
    /// This attribute should decorate each event listening class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventListenerAttribute : Attribute
    {
    }
}
