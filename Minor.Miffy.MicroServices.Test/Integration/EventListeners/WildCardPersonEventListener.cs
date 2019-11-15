using Minor.Miffy.MicroServices.Test.Integration.Events;

namespace Minor.Miffy.MicroServices.Test.Integration.EventListeners
{
    [EventListener("PeopleApp.Persons.Wild")]
    public class WildCardPersonEventListener
    {
        /// <summary>
        /// Reset the static variable
        /// </summary>
        public WildCardPersonEventListener() => ResultEvent = null;

        /// <summary>
        /// Result of the initial call from rabbitMQ
        /// </summary>
        internal static PersonAddedEvent ResultEvent { get; set; }
        
        /// <summary>
        /// Listener for the topic on the queue
        /// </summary>
        [Topic("PeopleApp.Persons.*")]
        public void Handles(PersonAddedEvent addedEvent) => ResultEvent = addedEvent;
    }
}