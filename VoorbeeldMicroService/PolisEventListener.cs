using Minor.Miffy.MicroServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoorbeeldMicroService
{
    [EventListener(queueName:"MVM.TestService.PolisEventListenerQueue")]
    public class PolisEventListener
    {
        private readonly IDbContextOptions<PolisContext> _context;

        public PolisEventListener(IDbContextOptions<PolisContext> context)
        {
            _context = context;
        }

        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _context.Polissen.Add(evt.Polis);
            _context.SaveChanges();
        }
    }

}
