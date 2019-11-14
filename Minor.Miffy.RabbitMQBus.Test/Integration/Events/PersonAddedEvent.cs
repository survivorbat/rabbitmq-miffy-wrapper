using Minor.Miffy.MicroServices;
using Minor.Miffy.RabbitMQBus.Test.Integration.Models;

namespace Minor.Miffy.RabbitMQBus.Test.Integration.Events
{
    public class PersonAddedEvent : DomainEvent
    {
        public PersonAddedEvent() : base("PeopleApp.Persons.New") { }
        public Person Person { get; set; }
    }
}