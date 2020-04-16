using System;

namespace Miffy
{
    public class CommandMessage
    {
        public string DestinationQueue { get; set; }
        public string ReplyQueue { get; set; }
        public Guid CorrelationId { get; set; }
        public long Timestamp { get; set; }
        public byte[] Body { get; set; }
        public string EventType { get; set; }
    }
}
