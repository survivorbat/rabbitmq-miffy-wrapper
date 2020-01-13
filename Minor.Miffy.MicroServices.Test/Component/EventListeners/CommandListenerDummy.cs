using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    public class CommandListenerDummy
    {
        [CommandListener("command.queue")]
        public DummyCommand Handle(DummyCommand eventCommand)
        {
            HandlesResult = eventCommand;
            return eventCommand;
        }

        public static DummyCommand HandlesResult { get; set; }
    }
}