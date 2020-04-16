using Miffy.MicroServices.Commands;

namespace Miffy.Microservices.Test.Integration.Integration.Events
{
    public class DummyCommand : DomainCommand
    {
        public string ExceptionMessage { get; }

        public DummyCommand(string message) : base("Test.Command.Listener")
        {
            ExceptionMessage = message;
        }

        public override string ToString()
        {
            return $"Dummy with message {ExceptionMessage}";
        }
    }
}
