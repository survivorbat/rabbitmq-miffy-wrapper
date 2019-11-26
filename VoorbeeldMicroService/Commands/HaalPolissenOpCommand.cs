using System.Collections.Generic;
using Minor.Miffy.MicroServices.Commands;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.Commands
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
        public HaalPolissenOpCommand() : base(QueueNames.HaalPolissenOpQueue) { }
    }
}