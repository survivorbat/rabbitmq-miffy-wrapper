using Minor.Miffy.MicroServices.Test.Integration.Models;

namespace Minor.Miffy.MicroServices.Test.Integration.Events
{
    public class CatAddedEvent : DomainEvent
    {
        public CatAddedEvent() : base("PeopleApp.Cats.New") { }

        public Cat Cat { get; set; }
        
        public override bool Equals(object obj) => obj is CatAddedEvent addedEvent && Equals(addedEvent);
        private bool Equals(CatAddedEvent other) => Equals(Cat, other.Cat);
        public override int GetHashCode() => Id.GetHashCode() ^ Timestamp.GetHashCode() ^ Topic.GetHashCode() ^ Cat.GetHashCode();

    }
}