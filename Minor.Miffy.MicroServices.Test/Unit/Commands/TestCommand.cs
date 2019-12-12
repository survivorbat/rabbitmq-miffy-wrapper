using System;
using Minor.Miffy.MicroServices.Commands;

namespace Minor.Miffy.MicroServices.Test.Unit.Commands
{
    /// <summary>
    /// Testevent to instantiate abstract class DomainEvent
    /// </summary>
    internal class TestCommand : DomainCommand
    {
        /// <summary>
        /// Proxy the topic to the base class
        /// </summary>
        public TestCommand(string queue) : base(queue) { }

        /// <summary>
        /// Proxy the queue and guild to the base class
        /// </summary>
        public TestCommand(string queue, Guid guid) : base(queue, guid) { }

        /// <summary>
        /// A dummy datafield to allow serialization tests
        /// </summary>
        public string DataField { get; set; }
    }
}
