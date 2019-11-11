using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Miffy
{
    public interface IMessageReceiver : IDisposable
    {
        string QueueName { get; }
        IEnumerable<string> TopicFilters { get; }

        void StartReceivingMessages();
        void StartHandlingMessages(EventMessageReceivedCallback Callback);
    }

    public delegate void EventMessageReceivedCallback(EventMessage eventMessage);

}
