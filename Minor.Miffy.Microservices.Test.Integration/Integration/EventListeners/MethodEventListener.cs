using Minor.Miffy.MicroServices;

namespace Minor.Miffy.Microservices.Test.Integration.Integration.EventListeners
{
    [EventListener("PersonApp.Cats.Test")]
    public class MethodEventListener
    {
        public void NotRelevantMethod() {}
        public string NotRelevantProperty { get; set; }
        
        [Topic("testPattern")]
        public void Handle(DomainEvent @event) { }
    }
}