using System;
using Newtonsoft.Json;

namespace Miffy.MicroServices.Events
{
    /// <summary>
    /// Base class for all domain events.
    /// </summary>
    public class DomainEvent
    {
        /// <summary>
        /// The Topic is used by the underlying protocol to route events to subscribers
        /// </summary>
        [JsonProperty]
        public string Topic { get; internal set; }

        /// <summary>
        /// The Timestamp is set to the creation time of the domain event.
        /// </summary>
        [JsonProperty]
        public long Timestamp { get; internal set; }

        /// <summary>
        /// The ID uniquely identifies the domain event.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; internal set; }

        /// <summary>
        /// Id of the process
        /// </summary>
        [JsonProperty]
        public Guid ProcessId { get; protected set; }

        /// <summary>
        /// The type of the currently created event
        /// </summary>
        [JsonProperty]
        public string Type { get; }

        /// <summary>
        /// Creates a domain event by setting the topic and generating a timestamp.
        /// </summary>
        /// <param name="topic">The topic should be of the format domain.eventname</param>
        protected DomainEvent(string topic)
        {
            Topic = topic;
            Timestamp = DateTime.Now.Ticks;
            Id = Guid.NewGuid();
            Type = GetType().Name;
        }

        /// <summary>
        /// Create a domain event with a process id
        /// </summary>
        protected DomainEvent(string topic, Guid processId) : this(topic)
        {
            ProcessId = processId;
        }
    }
}
