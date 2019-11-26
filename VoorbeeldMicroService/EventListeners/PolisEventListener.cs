using System;
using Minor.Miffy.MicroServices.Events;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.DAL;
using VoorbeeldMicroService.Events;

namespace VoorbeeldMicroService.EventListeners
{
    /// <summary>
    /// Example class that demonstrates how to create an event listener
    ///
    /// NOTE:
    /// The base framework was originally provided by an external entity
    /// and was not entirely suitable for my personal implementation
    /// of the assignment.
    ///
    /// For this reason, creating a new event listener class for each
    /// event listener is quite excessive and this procces will be changed in the feature
    /// </summary>
    [EventListener(QueueNames.PolisListenerEventQueue)]
    public class PolisEventListener
    {
        /// <summary>
        /// Injected Database context
        /// </summary>
        private readonly PolisContext _context;

        /// <summary>
        /// Services that this handler requires
        /// </summary>
        /// <param name="context">Database Context</param>
        public PolisEventListener(PolisContext context) => _context = context;

        /// <summary>
        /// Command that handles the incoming event, in this case saving it
        /// </summary>
        /// <param name="evt">incoming event</param>
        [Topic(TopicNames.MvmPolisbeheerPolisToegevoegd)]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _context.Polissen.Add(evt.Polis);
            _context.SaveChanges();
        }
    }
}
