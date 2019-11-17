using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    [EventListener("PersonApp.Cats.Test")]
    public class MethodEventListener
    {
        public void NotRelevantMethod() {}
        public string NotRelevantProperty { get; set; }
        
        [Topic("testPattern")]
        public void Handle(DummyEvent @event) { }
    }
}