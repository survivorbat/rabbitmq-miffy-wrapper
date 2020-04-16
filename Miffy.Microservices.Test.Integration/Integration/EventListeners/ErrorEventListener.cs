using System;
using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;
using Miffy.Microservices.Test.Integration.Integration.Events;

namespace Miffy.Microservices.Test.Integration.Integration.EventListeners
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