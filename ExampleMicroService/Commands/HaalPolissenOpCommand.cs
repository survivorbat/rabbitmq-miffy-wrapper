using System.Collections.Generic;
using ExampleMicroService.Models;
using Minor.Miffy.MicroServices.Commands;

namespace ExampleMicroService.Commands
{
    /// <summary>
    /// An example command that inherits from the DomainCommand
    /// </summary>
    public class HaalPolissenOpCommand : DomainCommand
    {
        /// <summary>
        /// A list of Polisses that is initially empty and we intend
        /// to fill in the destination service that will receive this
        /// command.
        /// </summary>
        public IEnumerable<Polis> Polisses = new List<Polis>();

        /// <summary>
        /// To ensure all HaalPolissenOpCommands have the same destination queue,
        /// we pass the name of the queue in the constructor.
        ///
        /// This is essentially the queue that this command will be sent to
        /// </summary>
        public HaalPolissenOpCommand() : base("MVM.TestService.HaalPolissenOpQueue") { }
    }
}