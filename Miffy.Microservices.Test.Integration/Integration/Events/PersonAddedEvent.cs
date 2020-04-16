using Miffy.MicroServices.Events;
using Miffy.Microservices.Test.Integration.Integration.Models;

namespace Miffy.Microservices.Test.Integration.Integration.Events
{
    public class PersonAddedEvent : DomainEvent
    {
        public PersonAddedEvent() : base("PeopleApp.Persons.New")
        {

        }

        public Person Person { get; set; }

        public override string ToString()
        {
            return $"Person with name {Person.FirstName} {Person.LastName}, Email {Person.Email} and Phone {Person.PhoneNumber}";
        }

        public override bool Equals(object obj)
        {
            return obj is PersonAddedEvent addedEvent && Equals(addedEvent);
        }

        private bool Equals(PersonAddedEvent other)
        {
            return Equals(Person, other.Person);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Timestamp.GetHashCode() ^ Topic.GetHashCode() ^ Person.GetHashCode();
        }
    }
}
