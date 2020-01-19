using System;
using System.Collections.Generic;

namespace Minor.Miffy.MicroServices.Host.HostEventArgs
{
    public class EventMessageHandledEventArgs : EventArgs
    {
        public string QueueName { get; set; }
        public IEnumerable<string> TopicExpressions { get; set; }
    }
}
