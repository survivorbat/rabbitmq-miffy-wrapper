using Miffy.MicroServices.Events;

namespace Miffy.MicroServices.Test.Component.EventListeners
{
    public class WrongParameterAmountEventListener
    {
        [EventListener]
        public void Handle(DummyEvent ev1, DummyEvent ev2)
        {

        }
    }
}
