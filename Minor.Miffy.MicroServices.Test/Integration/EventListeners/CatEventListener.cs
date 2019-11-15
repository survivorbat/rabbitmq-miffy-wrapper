using Minor.Miffy.MicroServices.Test.Integration.Events;

namespace Minor.Miffy.MicroServices.Test.Integration.EventListeners
{
    [EventListener("PeopleApp.Cats")]
    public class CatEventListener
    {
        /// <summary>
        /// Reset the static variable
        /// </summary>
        public CatEventListener() => ResultEvent = null;
        
        /// <summary>
        /// Static variable to keep track of the event
        /// </summary>
        internal static CatAddedEvent ResultEvent { get; private set; }
        
        /// <summary>
        /// Listener for the event
        /// </summary>
        [Topic("PeopleApp.Cats.New")]
        public void Handles(CatAddedEvent addedEvent) => ResultEvent = addedEvent;
    }
}