using Minor.Miffy.MicroServices.Commands;

namespace ExampleMicroService.Commands
{
    /// <summary>
    /// Example command that is send to the exception command
    /// </summary>
    public class ExceptionCommand : DomainCommand
    {
        /// <summary>
        /// Exception command to demonstrate how exceptions are thrown
        /// </summary>
        public ExceptionCommand() : base("exception.test") { }
    }
}