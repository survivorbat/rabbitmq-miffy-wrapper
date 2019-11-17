using Minor.Miffy.MicroServices.Events;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.Events
{
    public class PolisToegevoegdEvent : DomainEvent
    {
        public PolisToegevoegdEvent() : base(TopicNames.MvmPolisbeheerPolisToegevoegd) { }

        public Polis Polis { get; set; }
    }
}