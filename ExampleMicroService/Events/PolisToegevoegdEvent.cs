using ExampleMicroService.Models;
using Miffy.MicroServices.Events;

namespace ExampleMicroService.Events
{
    /// <summary>
    /// An example event to demonstrate how to
    /// create your own command
    /// </summary>
    public class PolisToegevoegdEvent : DomainEvent
    {
        /// <summary>
        /// Ensure that all PolisToegevoegdEvents have the same topic by putting it in the
        /// parent constructor
        /// </summary>
        public PolisToegevoegdEvent() : base("MVM.Polisbeheer.PolisToegevoegd") { }

        /// <summary>
        /// A model object that will be transmitted through the bus
        /// </summary>
        public Polis Polis { get; set; }
    }
}