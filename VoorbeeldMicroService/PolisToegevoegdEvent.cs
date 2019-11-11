using Minor.Miffy.MicroServices;

namespace VoorbeeldMicroService
{
    public class PolisToegevoegdEvent : DomainEvent
    {
        public PolisToegevoegdEvent() : base("MVM.Polisbeheer.PolisToegevoegd")
        {
        }
    }
}