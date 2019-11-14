using Minor.Miffy.MicroServices.Test.Integration.Events;

namespace Minor.Miffy.MicroServices.Test.Integration.EventListeners
{
    [EventListener("PeopleApp.Persons")]
    public class PersonEventListener
    {
        internal static PersonAddedEvent ResultEvent { get; private set; }
        
        [Topic("PeopleApp.Persons.New")]
        public void Handles(PersonAddedEvent addedEvent) => ResultEvent = addedEvent;
    }
}