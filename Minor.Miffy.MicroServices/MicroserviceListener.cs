using System.Collections.Generic;

namespace Minor.Miffy.MicroServices
{
    public class MicroserviceListener
    {
        public string Queue { get; set; }
        public IEnumerable<string> TopicExpressions { get; set; } = new List<string>();
        public EventMessageReceivedCallback Callback { get; set; }
    }
}