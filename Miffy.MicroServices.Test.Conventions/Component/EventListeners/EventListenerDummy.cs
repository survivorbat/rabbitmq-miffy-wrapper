using Miffy.MicroServices.Events;
using Miffy.MicroServices.Test.Conventions.Component.Event;

namespace Miffy.MicroServices.Test.Conventions.Component.EventListeners
{
    public class EventListenerDummy
    {
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
