using System;
using Newtonsoft.Json;

namespace Minor.Miffy.MicroServices.Events
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
        protected internal string Topic { get; internal set; }

        /// <summary>
        /// The Timestamp is set to the creation time of the domain event.
        /// </summary>
        [JsonProperty]
        protected internal long Timestamp { get; internal set; }

        /// <summary>
        /// The ID uniquely identifies the domain event.
        /// </summary>
        [JsonProperty]
        protected internal Guid Id { get; internal set; }

        /// <summary>
        /// Creates a domain event by setting the topic and generating a timestamp.
        /// </summary>
        /// <param name="topic">The topic should be of the format domain.eventname</param>
        protected internal DomainEvent(string topic)
        {
            Topic = topic;
            Timestamp = DateTime.Now.Ticks;
            Id = Guid.NewGuid();
        }
    }
}
