using System.Diagnostics.Tracing;
using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    [CommandListener("command.queue")]
    public class CommandListenerDummy
    {
        public DummyCommand Handle(DummyCommand eventCommand)
        {
            HandlesResult = eventCommand;
            return eventCommand;
        }

        public static DummyCommand HandlesResult { get; set; }
    }
}