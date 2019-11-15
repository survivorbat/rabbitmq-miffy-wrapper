using Minor.Miffy.MicroServices.Test.Integration.Events;

namespace Minor.Miffy.MicroServices.Test.Integration.EventListeners
{
    [EventListener("PeopleApp.Persons.Wild2")]
    public class WildCardPersonEventListener2
    {
        /// <summary>
        /// Reset the static variable
        /// </summary>
        public WildCardPersonEventListener2() => ResultEvent = null;
        
        /// <summary>
        /// To keep track of this event
        /// </summary>
        internal static PersonAddedEvent ResultEvent { get; set; }
        
        /// <summary>
        /// Listener for events
        /// </summary>
        [Topic("PeopleApp.#")]
        public void Handles(PersonAddedEvent addedEvent) => ResultEvent = addedEvent;
    }
}