using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    public class WrongReturnCommandListener
    {
        [CommandListener("somequeue")]
        public int Handle(DummyCommand command)
        {
            return 5;
        }
    }
}
