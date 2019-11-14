using System;

namespace Minor.Miffy
{
    public class CommandMessage
    {
        public string ReplyQueue { get; set; }
        public string DestinationQueue { get; set; }
        public Guid CorrelationId { get; set; }
        public long Timestamp { get; set; }
        public string EventType { get; set; }
        public byte[] Body { get; set; }
    }
}