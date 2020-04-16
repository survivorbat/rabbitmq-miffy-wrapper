using System.Collections.Generic;
using Miffy.MicroServices.Events;
using Miffy.Microservices.Test.Integration.Integration.Events;

namespace Miffy.Microservices.Test.Integration.Integration.EventListeners
{
    public class SpamEventListener
    {
        /// <summary>
        /// Static variable to keep the result in
        /// </summary>
        internal static List<CatAddedEvent> ResultEvents { get; set; } = new List<CatAddedEvent>();

        /// <summary>
        /// Listener for all events
        /// </summary>
        [EventListener]
        [Topic("PeopleApp.Cats.New")]
        public void Handles(CatAddedEvent addedEvent)
        {
            ResultEvents.Add(addedEvent);
        }
    }
}
