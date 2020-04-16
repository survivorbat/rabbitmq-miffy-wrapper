using System;
using Miffy.MicroServices.Events;
using Miffy.Microservices.Test.Integration.Integration.Events;

namespace Miffy.Microservices.Test.Integration.Integration.EventListeners
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
        [EventListener]
        [Topic("PeopleApp.Persons.New")]
        public void Handles(PersonAddedEvent addedEvent)
        {
            ResultEvent = addedEvent;
        }
    }
}
