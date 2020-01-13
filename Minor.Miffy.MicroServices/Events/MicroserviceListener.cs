using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Minor.Miffy.MicroServices.Events
{
    public class MicroserviceListener
    {
        public IEnumerable<string> TopicExpressions { get; set; } = new List<string>();
        public IEnumerable<Regex> TopicRegularExpressions { get; set; } = new List<Regex>();
        public EventMessageReceivedCallback Callback { get; set; }
    }
}
