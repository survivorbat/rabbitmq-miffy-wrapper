using ExampleMicroService.DAL;
using ExampleMicroService.Events;
using Minor.Miffy.MicroServices.Events;

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
        /// Services that this handler requires
        /// </summary>
        /// <param name="context">Database Context</param>
        public ExampleEventListener(PolisContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Command that handles the incoming event, in this case saving it
        /// </summary>
        /// <param name="evt">incoming event</param>
        [EventListener("MVM.TestService.PolisEventListenerQueue")]
        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _context.Polissen.Add(evt.Polis);
        }
    }
}
