using Minor.Miffy.MicroServices.Commands;

namespace Minor.Miffy.MicroServices.Test.Conventions.Component.Commands
{
    public class GetAnimalsCommand : DomainCommand
    {
        public GetAnimalsCommand() : base("command.test")
        {
        }
    }
}
