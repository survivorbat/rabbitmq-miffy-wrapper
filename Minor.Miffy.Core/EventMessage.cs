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
        public string Topic { get; set; }
        
        [JsonProperty]
        public Guid CorrelationId { get; set; }
        
        [JsonProperty]
        public long Timestamp { get; set; }
        
        [JsonProperty]
        public string EventType { get; set; }
        
        [JsonProperty]
        public byte[] Body { get; set; }
    }
}