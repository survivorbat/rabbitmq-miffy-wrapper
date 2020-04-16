using System.Collections.Concurrent;
using System.Threading;

namespace Miffy.TestBus
{
    /// <summary>
    /// Wrapper class that contains a reset event, a queue and a topicname
    /// </summary>
    internal class TestBusQueueWrapper<T>
    {
        /// <summary>
        /// Reset event to wait for
        /// </summary>
        internal AutoResetEvent AutoResetEvent { get; } = new AutoResetEvent(false);
        
        /// <summary>
        /// The actual queue with messages
        /// </summary>
        internal ConcurrentQueue<T> Queue { get; } = new ConcurrentQueue<T>();
    }
}