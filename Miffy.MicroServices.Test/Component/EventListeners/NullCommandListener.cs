using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;

namespace Miffy.MicroServices.Test.Component.EventListeners
{
    public class NullCommandListener
    {
        [CommandListener("test.queue")]
        public DummyCommand Handles(DummyCommand command)
        {
            return null;
        }
    }
}
