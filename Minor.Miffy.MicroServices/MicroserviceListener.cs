using System.Collections.Generic;

namespace Minor.Miffy.MicroServices
{
    public class MicroserviceListener
    {
        internal string Queue { get; set; }
        internal IEnumerable<string> TopicExpressions { get; set; } = new List<string>();
        internal EventMessageReceivedCallback Callback { get; set; }
    }
}