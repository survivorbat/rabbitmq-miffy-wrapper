using System.Collections.Concurrent;
using System.Threading;

namespace Minor.Miffy.TestBus
{
    /// <summary>
    /// Wrapper class that contains a reset event, a queue and a topicname
    /// </summary>
    internal class TestBusQueueWrapper
    {
        /// <summary>
        /// Reset event to wait for
        /// </summary>
        internal AutoResetEvent AutoResetEvent { get; } = new AutoResetEvent(false);
        
        /// <summary>
        /// The actual queue with messages
        /// </summary>
        internal ConcurrentQueue<EventMessage> Queue { get; } = new ConcurrentQueue<EventMessage>();
    }
}