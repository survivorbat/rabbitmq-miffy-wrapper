using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;

namespace Miffy.MicroServices.Test.Component.EventListeners
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
