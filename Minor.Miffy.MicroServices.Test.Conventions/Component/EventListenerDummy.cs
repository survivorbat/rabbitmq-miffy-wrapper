using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Conventions.Component
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
        [EventListener(queueName: "TestQueue")]
        [Topic("TestTopic")]
        public void Handles(DummyEvent dummyEvent) => HandlesResult = dummyEvent;
    }
}