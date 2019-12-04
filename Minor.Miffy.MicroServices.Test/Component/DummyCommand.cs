using Minor.Miffy.MicroServices.Commands;

namespace Minor.Miffy.MicroServices.Test.Component
{
    public class DummyCommand : DomainCommand
    {
        public string Text { get; set; }

        public DummyCommand() : base("test.queue")
        {
        }
    }
}