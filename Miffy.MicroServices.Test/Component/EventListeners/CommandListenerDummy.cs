using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;

namespace Miffy.MicroServices.Test.Component.EventListeners
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