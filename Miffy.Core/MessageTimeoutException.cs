using System;

namespace Miffy
{
    /// <summary>
    /// If a message is not received within a specific time limit this exception will be thrown
    /// </summary>
    public class MessageTimeoutException : Exception
    {
        /// <summary>
        /// The timeout given for the response
        /// </summary>
        public virtual int TimeOut { get;  }

        /// <summary>
        /// Create a new timeout exception with a given message and a timeout
        /// </summary>
        public MessageTimeoutException(string message, int timeout) : base(message) => TimeOut = timeout;
    }
}
