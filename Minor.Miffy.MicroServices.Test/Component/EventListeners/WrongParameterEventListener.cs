using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    public class WrongParameterEventListener
    {
        [EventListener("somequeue")]
        public void Handle(string test, string test2)
        {

        }
    }
}
