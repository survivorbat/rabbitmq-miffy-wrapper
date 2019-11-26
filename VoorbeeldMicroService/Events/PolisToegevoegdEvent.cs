using Minor.Miffy.MicroServices.Events;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.Events
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
        public PolisToegevoegdEvent() : base(TopicNames.MvmPolisbeheerPolisToegevoegd) { }

        /// <summary>
        /// A model object that will be transmitted through the bus
        /// </summary>
        public Polis Polis { get; set; }
    }
}