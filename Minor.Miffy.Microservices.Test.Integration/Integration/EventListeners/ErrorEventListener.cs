using System;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.Microservices.Test.Integration.Integration.Events;

namespace Minor.Miffy.Microservices.Test.Integration.Integration.EventListeners
{
    [CommandListener("Test.Command.Listener")]
    public class ErrorEventListener
    {
        public DummyCommand Handles(DummyCommand command) => throw new Exception(command.ExceptionMessage);
    }
}