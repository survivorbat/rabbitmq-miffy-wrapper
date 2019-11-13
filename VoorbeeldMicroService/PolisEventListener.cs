using Minor.Miffy.MicroServices;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.DAL;

namespace VoorbeeldMicroService
{
    [EventListener(QueueNames.PolisListenerEventQueue)]
    public class PolisEventListener
    {
        private readonly PolisContext _context;

        public PolisEventListener(PolisContext context) => _context = context;

        [Topic(TopicNames.MvmPolisbeheerPolisToegevoegd)]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _context.Polissen.Add(evt.Polis);
            _context.SaveChanges();
        }
    }
}
