using Minor.Miffy.MicroServices.Commands;

namespace Minor.Miffy.MicroServices.Test.Component
{
    public class DummyCommand : DomainCommand
    {
        public string Text { get; set; }

        protected DummyCommand() : base("test.queue")
        {
        }
    }
}