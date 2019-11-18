using System.Collections.Generic;
using System.Linq;
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

        public HaalPolissenOpCommandListener(PolisContext context) => _context = context;

        public HaalPolissenOpCommand Handle(HaalPolissenOpCommand command) => new HaalPolissenOpCommand { Polisses = _context.Polissen.ToArray() };
    }
}