using Miffy.MicroServices.Events;

namespace Miffy.MicroServices.Test.Component.EventListeners
{
    public class EventListenerDummy
    {
        /// <summary>
        /// Reset the result
        /// </summary>
        public EventListenerDummy()
        {
            HandlesResult = null;
        }

        /// <summary>
        /// Result of the handles method
        /// </summary>
        public static DummyEvent HandlesResult { get; internal set; }

        /// <summary>
        /// Put the result in a static variable so we can use it in tests
        /// </summary>
        [EventListener]
        [Topic("TestTopic")]
        public void Handles(DummyEvent dummyEvent) => HandlesResult = dummyEvent;
    }
}
