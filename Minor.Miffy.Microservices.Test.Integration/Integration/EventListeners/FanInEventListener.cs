using Minor.Miffy.Microservices.Test.Integration.Integration.Events;

namespace Minor.Miffy.MicroServices.Test.Integration.EventListeners
{
    [EventListener("PeopleApp.Persons.FanIn")]
    public class FanInEventListener
    {
        /// <summary>
        /// Reset the static variable
        /// </summary>
        public FanInEventListener() => ResultEvent = null;
        
        /// <summary>
        /// Static variable to keep the result in
        /// </summary>
        internal static PersonAddedEvent ResultEvent { get; set; }
        
        /// <summary>
        /// Listener for all events
        /// </summary>
        [Topic("#")]
        public void Handles(PersonAddedEvent addedEvent) => ResultEvent = addedEvent;
    }
}