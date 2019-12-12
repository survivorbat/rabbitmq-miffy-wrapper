using System;
using System.Collections.Generic;

namespace Minor.Miffy
{
    public interface IMessageReceiver : IDisposable
    {
        string QueueName { get; }
        bool IsPaused { get; }

        IEnumerable<string> TopicFilters { get; }

        void StartReceivingMessages();
        void StartHandlingMessages(EventMessageReceivedCallback callback);

        void Pause();
        void Resume();
    }

    public delegate void EventMessageReceivedCallback(EventMessage eventMessage);
}
