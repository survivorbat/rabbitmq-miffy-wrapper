using System;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.Microservices.Test.Integration.Integration.Events;

namespace Minor.Miffy.Microservices.Test.Integration.Integration.EventListeners
{
    public class ErrorEventListener
    {    
        [CommandListener("Test.Command.Listener")]
        public DummyCommand Handles(DummyCommand command)
        {
            throw new Exception(command.ExceptionMessage);
        }
    }
}