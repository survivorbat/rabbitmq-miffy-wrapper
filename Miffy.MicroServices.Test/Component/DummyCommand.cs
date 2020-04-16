using Miffy.MicroServices.Commands;

namespace Miffy.MicroServices.Test.Component
{
    public class DummyCommand : DomainCommand
    {
        public string Text { get; set; }

        public DummyCommand() : base("test.queue")
        {
        }
    }
}