using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    public class WrongParameterAmountEventListener
    {
        [EventListener("somequeue")]
        public void Handle(DummyEvent ev1, DummyEvent ev2)
        {

        }
    }
}