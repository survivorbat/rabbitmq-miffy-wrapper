using System;
using Minor.Miffy.MicroServices.Events;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.DAL;
using VoorbeeldMicroService.Events;

namespace VoorbeeldMicroService.EventListeners
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
