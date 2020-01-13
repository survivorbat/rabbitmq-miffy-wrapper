using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    public class MethodEventListener
    {
        public void NotRelevantMethod()
        {

        }

        public string NotRelevantProperty { get; set; }

        [EventListener]
        [Topic("testPattern")]
        public void Handle(DummyEvent @event)
        {
            // Nothing
        }
    }
}
