using System;

namespace Minor.Miffy
{
    public class EventMessage
    {
        public string Topic { get; set; }
        public Guid CorrelationId { get; set; }
        public long Timestamp { get; set; }
        public string EventType { get; set; }
        public byte[] Body { get; set; }
    }
}