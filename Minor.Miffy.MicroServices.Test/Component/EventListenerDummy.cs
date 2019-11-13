namespace Minor.Miffy.MicroServices.Test.Component
{
    [EventListener(queueName: "TestQueue")]
    public class EventListenerDummy
    {
        /// <summary>
        /// Result of the handles method
        /// </summary>
        public static DummyEvent HandlesResult { get; internal set; }
        
        /// <summary>
        /// Put the result in a static variable so we can use it in tests
        /// </summary>
        [Topic("TestTopic")]
        public void Handles(DummyEvent dummyEvent) => HandlesResult = dummyEvent;
    }
}