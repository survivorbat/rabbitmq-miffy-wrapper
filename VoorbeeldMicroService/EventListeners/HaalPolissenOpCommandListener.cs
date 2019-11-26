using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices.Events;
using VoorbeeldMicroService.Commands;
using VoorbeeldMicroService.Constants;
using VoorbeeldMicroService.DAL;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.EventListeners
{
    /// <summary>
    /// An example command listeners that listens for incoming commands.
    ///
    /// NOTE:
    /// The base framework was originally provided by an external entity
    /// and was not entirely suitable for my personal implementation
    /// of the assignment.
    ///
    /// For this reason, creating a new ocmmand listener class for each
    /// command listeners is quite excessive and this procces will be changed in the feature
    /// </summary>
    [CommandListener(QueueNames.HaalPolissenOpQueue)]
    public class HaalPolissenOpCommandListener
    {
        /// <summary>
        /// Database context as an example
        /// </summary>
        private readonly PolisContext _context;

        /// <summary>
        /// Constructor with injected dependencies
        /// </summary>
        /// <param name="context">Database Context</param>
        public HaalPolissenOpCommandListener(PolisContext context) => _context = context;

        /// <summary>
        /// Receives a command, then returns a changed command or a entirely new one
        /// </summary>
        /// <param name="command">Received command from a certain source</param>
        /// <returns>A new or modified command with new data</returns>
        public HaalPolissenOpCommand Handle(HaalPolissenOpCommand command) => new HaalPolissenOpCommand { Polisses = _context.Polissen.ToArray() };
    }
}