namespace Minor.Miffy.MicroServices.Test.Unit
{
    /// <summary>
    /// Testevent to instantiate abstract class DomainEvent
    /// </summary>
    internal class TestEvent : DomainEvent
    {
        /// <summary>
        /// Proxy the topic to the base class
        /// </summary>
        public TestEvent(string topic) : base(topic) { }
        
        /// <summary>
        /// A dummy datafield to allow serialization tests
        /// </summary>
        public string DataField { get; set; }
    }
}