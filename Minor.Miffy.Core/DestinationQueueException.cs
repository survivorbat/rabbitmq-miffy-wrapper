using System;

namespace Minor.Miffy
{
    /// <summary>
    /// An exception happened on an external queue
    /// </summary>
    [Serializable]
    public class DestinationQueueException : Exception
    {
        public string ReplyQueueName { get; }
        public string DestinationQueueName { get; }
        public Guid CorrelationId { get; }
        
        /// <summary>
        /// Create an exception instance with a message
        /// </summary>
        /// <param name="message">Message received from the destination queue</param>
        public DestinationQueueException(string message) : base(message) { }

        /// <summary>
        /// Create an exception instance with a message, queues and correlation id
        /// </summary>
        /// <param name="message">Message of the error</param>
        /// <param name="innerException">The inner exception</param>
        /// <param name="replyQueueName">Name of the reply queue</param>
        /// <param name="destinationQueueName">Name of the destination queue</param>
        /// <param name="correlationId">Correlation ID</param>
        public DestinationQueueException(string message, Exception innerException, string replyQueueName, 
            string destinationQueueName, Guid correlationId) : base(message, innerException)
        {
            ReplyQueueName = replyQueueName;
            DestinationQueueName = destinationQueueName;
            CorrelationId = correlationId;
        }
    }
}