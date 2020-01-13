using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
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
