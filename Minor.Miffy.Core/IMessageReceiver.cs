using System;
using System.Collections.Generic;

namespace Minor.Miffy
{
    public interface IMessageReceiver : IDisposable
    {
        string QueueName { get; }
        IEnumerable<string> TopicFilters { get; }

        void StartReceivingMessages();
        void StartHandlingMessages(EventMessageReceivedCallback callback);
    }

    public delegate void EventMessageReceivedCallback(EventMessage eventMessage);

}
