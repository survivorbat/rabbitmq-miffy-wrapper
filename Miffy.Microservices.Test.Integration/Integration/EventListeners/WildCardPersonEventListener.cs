using Miffy.MicroServices.Events;
using Miffy.Microservices.Test.Integration.Integration.Events;

namespace Miffy.Microservices.Test.Integration.Integration.EventListeners
{
    public class WildCardPersonEventListener
    {
        /// <summary>
        /// Reset the static variable
        /// </summary>
        public WildCardPersonEventListener()
        {
            ResultEvent = null;
        }

        /// <summary>
        /// Result of the initial call from rabbitMQ
        /// </summary>
        internal static PersonAddedEvent ResultEvent { get; set; }

        /// <summary>
        /// Listener for the topic on the queue
        /// </summary>
        [EventListener]
        [Topic("PeopleApp.Persons.*")]
        public void Handles(PersonAddedEvent addedEvent)
        {
            ResultEvent = addedEvent;
        }
    }
}
