﻿using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PolisEventListener> _logger;

        public PolisEventListener(PolisContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<PolisEventListener>();
        }

        [Topic(TopicNames.MvmPolisbeheerPolisToegevoegd)]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _logger.LogDebug($"Received PolisToegevoegdEvent with id {evt.Id}");
            
            _context.Polissen.Add(evt.Polis);
            _context.SaveChanges();
        }
    }
}