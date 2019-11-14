using System;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.DAL;

namespace VoorbeeldMicroService
{
    [EventListener(QueueNames.PolisListenerEventQueue)]
    public class PolisEventListener
    {
        private readonly PolisContext _context;
        private readonly ILogger<PolisEventListener> _logger;

        public PolisEventListener(PolisContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<PolisEventListener>();
        }

        [Topic(TopicNames.MvmPolisbeheerPolisToegevoegd)]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _logger.LogDebug("Received PolisToegevoegdEvent!");
            _context.Polissen.Add(evt.Polis);
            _context.SaveChanges();
        }
    }
}
