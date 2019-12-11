using System;
using Newtonsoft.Json;

namespace Minor.Miffy
{
    /// <summary>
    /// Low-level message that gets send over the bus
    /// </summary>
    public class EventMessage
    {
        [JsonProperty]
        public virtual string Topic { get; set; }

        [JsonProperty]
        public virtual Guid CorrelationId { get; set; }

        [JsonProperty]
        public virtual long Timestamp { get; set; }

        [JsonProperty]
        public virtual string EventType { get; set; }

        [JsonProperty]
        public virtual byte[] Body { get; set; }
    }
}
