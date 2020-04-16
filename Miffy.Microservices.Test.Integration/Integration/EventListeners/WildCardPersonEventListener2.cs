using Miffy.MicroServices.Events;
using Miffy.Microservices.Test.Integration.Integration.Events;

namespace Miffy.Microservices.Test.Integration.Integration.EventListeners
{
    public class WildCardPersonEventListener2
    {
        /// <summary>
        /// Reset the static variable
        /// </summary>
        public WildCardPersonEventListener2()
        {
            ResultEvent = null;
        }

        /// <summary>
        /// To keep track of this event
        /// </summary>
        internal static PersonAddedEvent ResultEvent { get; set; }

        /// <summary>
        /// Listener for events
        /// </summary>
        [EventListener]
        [Topic("PeopleApp.#")]
        public void Handles(PersonAddedEvent addedEvent)
        {
            ResultEvent = addedEvent;
        }
    }
}
