using System;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.Microservices.Test.Integration.Integration.Events;

namespace Minor.Miffy.Microservices.Test.Integration.Integration.EventListeners
{
    public class PersonEventListener
    {
        /// <summary>
        /// Reset the static variable
        /// </summary>
        public PersonEventListener()
        {
            ResultEvent = null;
        }

        /// <summary>
        /// Static variable to keep track of the event
        /// </summary>
        internal static PersonAddedEvent ResultEvent { get; set; }

        /// <summary>
        /// Listener for the event
        /// </summary>
        [EventListener("PeopleApp.Persons")]
        [Topic("PeopleApp.Persons.New")]
        public void Handles(PersonAddedEvent addedEvent)
        {
            ResultEvent = addedEvent;
        }
    }
}
