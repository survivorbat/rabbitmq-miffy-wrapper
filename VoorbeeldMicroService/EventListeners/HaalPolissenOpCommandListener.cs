using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices.Events;
using VoorbeeldMicroService.Commands;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.DAL;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.EventListeners
{
    [CommandListener(QueueNames.HaalPolissenOpQueue)]
    public class HaalPolissenOpCommandListener
    {
        private readonly PolisContext _context;

        private readonly ILogger<HaalPolissenOpCommandListener> _logger;
        
        public HaalPolissenOpCommandListener(PolisContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<HaalPolissenOpCommandListener>();
        }

        public HaalPolissenOpCommand Handle(HaalPolissenOpCommand command)
        {
            _logger.LogInformation($"Received haalPolossenOpCommand with Id {command.Id} and queue {command.DestinationQueue}");
            return new HaalPolissenOpCommand {Polisses = new List<Polis> {new Polis {Klantnaam = "Jan"}}};
        }
    }
}