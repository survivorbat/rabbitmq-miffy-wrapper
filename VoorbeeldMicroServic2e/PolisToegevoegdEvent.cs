using Minor.Miffy.MicroServices;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService
{
    public class PolisToegevoegdEvent : DomainEvent
    {
        public PolisToegevoegdEvent() : base("MVM.Polisbeheer.PolisToegevoegd") { }

        public Polis Polis { get; set; }
    }
}