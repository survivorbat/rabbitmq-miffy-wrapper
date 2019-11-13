namespace Minor.Miffy.MicroServices.Test.Conventions.Component
{
    [EventListener(queueName: "TestConcurrentQueue")]
    public class ConcurrentEventListenerDummy
    {
        /// <summary>
        /// Result of the handles method
        /// </summary>
        public static DummyEvent HandlesResult { get; internal set; }
        
        /// <summary>
        /// Put the result in a static variable so we can use it in tests
        /// </summary>
        [Topic("ConcurrentTopic")]
        public void Handles(DummyEvent dummyEvent) => HandlesResult = dummyEvent;
    }
    
    [EventListener(queueName: "TestConcurrentQueue")]
    public class ConcurrentEventListenerDummy2
    {
        /// <summary>
        /// Result of the handles method
        /// </summary>
        public static DummyEvent HandlesResult { get; internal set; }
        
        /// <summary>
        /// Put the result in a static variable so we can use it in tests
        /// </summary>
        [Topic("ConcurrentTopic")]
        public void Handles(DummyEvent dummyEvent) => HandlesResult = dummyEvent;
    }
}