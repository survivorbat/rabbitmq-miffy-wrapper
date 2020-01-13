using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    public class StringEventListenerDummy
    {
        internal static string ReceivedData { get; set; }

        [EventListener]
        [Topic("test.topic")]
        public void Handles(string data)
        {
            ReceivedData = data;
        }
    }
}
