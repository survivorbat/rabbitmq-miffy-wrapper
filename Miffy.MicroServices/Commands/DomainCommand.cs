using System;
using Newtonsoft.Json;

namespace Miffy.MicroServices.Commands
{
    /// <summary>
    /// Base class for all domain events.
    /// </summary>
    public class DomainCommand
    {
        /// <summary>
        /// The Timestamp is set to the creation time of the domain event.
        /// </summary>
        [JsonProperty]
        public long Timestamp { get; protected set; }

        /// <summary>
        /// The ID uniquely identifies the domain event.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; protected set; }

        /// <summary>
        /// Id of the process
        /// </summary>
        [JsonProperty]
        public Guid ProcessId { get; protected set; }

        /// <summary>
        /// Queue to send message to
        /// </summary>
        [JsonProperty]
        public string DestinationQueue { get; protected set; }

        /// <summary>
        /// Creates a domain event by setting the topic and generating a timestamp.
        /// </summary>
        protected DomainCommand(string destinationQueue)
        {
            Timestamp = DateTime.Now.Ticks;
            Id = Guid.NewGuid();
            DestinationQueue = destinationQueue;
        }

        /// <summary>
        /// Create a domain command with a process id
        /// </summary>
        protected DomainCommand(string destinationQueue, Guid processId) : this(destinationQueue)
        {
            ProcessId = processId;
        }
    }
}
