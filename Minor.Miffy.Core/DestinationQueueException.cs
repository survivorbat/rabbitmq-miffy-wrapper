using System;

namespace Minor.Miffy
{
    /// <summary>
    /// An exception happened on an external queue
    /// </summary>
    [Serializable]
    public class DestinationQueueException : Exception
    {
        /// <summary>
        /// Create a destination queue exception with a message 
        /// </summary>
        public DestinationQueueException(string message) : base(message) { }
    }
}