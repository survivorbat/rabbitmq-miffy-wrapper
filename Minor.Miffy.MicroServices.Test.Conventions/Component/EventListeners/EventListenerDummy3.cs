using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Test.Conventions.Component.Event;

namespace Minor.Miffy.MicroServices.Test.Conventions.Component.EventListeners
{
    public class EventListenerDummy3
    {
        /// <summary>
        /// Result of the handles method
        /// </summary>
        public static DummyEvent HandlesResult { get; internal set; }

        /// <summary>
        /// Put the result in a static variable so we can use it in tests
        /// </summary>
        [EventListener]
        [Topic("IrrelevantTopic")]
        public void Handles(DummyEvent dummyEvent) => HandlesResult = dummyEvent;
    }
}
