using System;
using ExampleMicroService.DAL;
using ExampleMicroService.Events;
using Microsoft.Extensions.Logging;
using Miffy.MicroServices.Events;

namespace ExampleMicroService.EventListeners
{
    /// <summary>
    /// Example class that demonstrates how to create an event listener
    /// </summary>
    public class ExampleEventListener
    {
        /// <summary>
        /// Injected Database context
        /// </summary>
        private readonly PolisContext _context;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<ExampleEventListener> _logger;

        /// <summary>
        /// Services that this handler requires
        /// </summary>
        public ExampleEventListener(PolisContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<ExampleEventListener>();
        }

        /// <summary>
        /// Command that handles the incoming event, in this case saving it
        /// </summary>
        [EventListener]
        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _logger.LogInformation($"Received a new polis! Polis from customer {evt.Polis.Klantnaam}");
            _context.Polissen.Add(evt.Polis);
        }

        /// <summary>
        /// A string listener that gets the json result of the item
        /// </summary>
        [EventListener]
        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(string evt)
        {
            _logger.LogInformation($"Received a string event! String: {evt}");
            Console.WriteLine($"String listener received: {evt}");
        }
    }
}
