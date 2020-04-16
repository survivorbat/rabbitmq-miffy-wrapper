using Miffy.MicroServices.Commands;

namespace Miffy.MicroServices.Test.Conventions.Component.Commands
{
    public class GetAnimalsCommand : DomainCommand
    {
        public GetAnimalsCommand() : base("command.test")
        {
        }
    }
}
