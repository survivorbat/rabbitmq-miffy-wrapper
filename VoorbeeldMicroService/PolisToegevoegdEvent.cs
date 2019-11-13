using Minor.Miffy.MicroServices;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService
{
    public class PolisToegevoegdEvent : DomainEvent
    {
        public PolisToegevoegdEvent() : base(TopicNames.MvmPolisbeheerPolisToegevoegd) { }

        public Polis Polis { get; set; }
    }
}