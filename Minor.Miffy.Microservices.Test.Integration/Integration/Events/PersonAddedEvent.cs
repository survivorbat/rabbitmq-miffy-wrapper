using Minor.Miffy.MicroServices;
using Minor.Miffy.Microservices.Test.Integration.Integration.Models;

namespace Minor.Miffy.Microservices.Test.Integration.Integration.Events
{
    public class PersonAddedEvent : DomainEvent
    {
        public PersonAddedEvent() : base("PeopleApp.Persons.New") { }
        
        public Person Person { get; set; }
        
        public override bool Equals(object obj) => obj is PersonAddedEvent addedEvent && Equals(addedEvent);
        private bool Equals(PersonAddedEvent other) => Equals(Person, other.Person);
        public override int GetHashCode() => Id.GetHashCode() ^ Timestamp.GetHashCode() ^ Topic.GetHashCode() ^ Person.GetHashCode();
    }
}