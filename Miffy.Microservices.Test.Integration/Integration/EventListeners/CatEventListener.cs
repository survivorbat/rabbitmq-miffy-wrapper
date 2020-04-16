using Miffy.MicroServices.Events;
using Miffy.Microservices.Test.Integration.Integration.Events;

namespace Miffy.Microservices.Test.Integration.Integration.EventListeners
{
    public class CatEventListener
    {
        /// <summary>
        /// Reset the static variable
        /// </summary>
        public CatEventListener()
        {
            ResultEvent = null;
        }

        /// <summary>
        /// Static variable to keep track of the event
        /// </summary>
        internal static CatAddedEvent ResultEvent { get; set; }

        /// <summary>
        /// Listener for the event
        /// </summary>
        [EventListener]
        [Topic("PeopleApp.Cats.New")]
        public void Handles(CatAddedEvent addedEvent)
        {
            ResultEvent = addedEvent;
        }
    }
}
