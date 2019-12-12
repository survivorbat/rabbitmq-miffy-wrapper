using System;
using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Unit.Events
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

        public TestEvent(string topic, Guid guid) : base(topic, guid) {}

        /// <summary>
        /// A dummy datafield to allow serialization tests
        /// </summary>
        public string DataField { get; set; }
    }
}
