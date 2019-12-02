using Minor.Miffy.MicroServices.Commands;

namespace Minor.Miffy.Microservices.Test.Integration.Integration.Events
{
    public class DummyCommand : DomainCommand
    {
        public string ExceptionMessage { get; private set; }
        
        public DummyCommand(string message) : base("Test.Command.Listener")
        {
            ExceptionMessage = message;
        }
    }
}