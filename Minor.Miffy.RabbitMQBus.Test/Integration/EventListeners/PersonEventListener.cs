using Minor.Miffy.MicroServices;
using Minor.Miffy.RabbitMQBus.Test.Integration.Events;

namespace Minor.Miffy.RabbitMQBus.Test.Integration.EventListeners
{
    [EventListener("PeopleApp.Persons")]
    public class PersonEventListener
    {
        internal static PersonAddedEvent ResultEvent { get; private set; }
        
        [Topic("PeopleApp.Persons.New")]
        public void Handles(PersonAddedEvent addedEvent) => ResultEvent = addedEvent;
    }
}