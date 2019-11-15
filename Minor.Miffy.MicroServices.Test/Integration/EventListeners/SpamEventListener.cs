using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Minor.Miffy.MicroServices.Test.Integration.Events;

namespace Minor.Miffy.MicroServices.Test.Integration.EventListeners
{
    [EventListener("PeopleApp.Cats.Spam")]
    public class SpamEventListener
    {
        /// <summary>
        /// Static variable to keep the result in
        /// </summary>
        internal static ConcurrentBag<CatAddedEvent> ResultEvents { get; set; } = new ConcurrentBag<CatAddedEvent>();

        /// <summary>
        /// Listener for all events
        /// </summary>
        [Topic("PeopleApp.Cats.New")]
        public void Handles(CatAddedEvent addedEvent)
        {
            ResultEvents.Add(addedEvent);
        }
    }
}