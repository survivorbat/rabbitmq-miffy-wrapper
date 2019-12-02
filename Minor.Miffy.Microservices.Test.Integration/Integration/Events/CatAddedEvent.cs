using Minor.Miffy.MicroServices;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.Microservices.Test.Integration.Integration.Models;

namespace Minor.Miffy.Microservices.Test.Integration.Integration.Events
{
    public class CatAddedEvent : DomainEvent
    {
        public CatAddedEvent() : base("PeopleApp.Cats.New")
        {
            
        }

        public Cat Cat { get; set; }
        
        public override bool Equals(object obj)
        {
            return obj is CatAddedEvent addedEvent && Equals(addedEvent);
        }

        private bool Equals(CatAddedEvent other)
        {
            return Equals(Cat, other.Cat);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Timestamp.GetHashCode() ^ Topic.GetHashCode() ^ Cat.GetHashCode();
        }
    }
}