using ExampleMicroService.Commands;
using ExampleMicroService.DAL;
using ExampleMicroService.Exceptions;
using Minor.Miffy.MicroServices.Events;

namespace ExampleMicroService.EventListeners
{
    /// <summary>
    /// An example command listeners that listens for incoming commands.
    /// </summary>
    public class ExampleCommandListener
    {
        /// <summary>
        /// Database context as an example
        /// </summary>
        private readonly PolisContext _context;

        /// <summary>
        /// Constructor with injected dependencies
        /// </summary>
        /// <param name="context">Database Context</param>
        public ExampleCommandListener(PolisContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Receives a command, then returns a changed command
        /// </summary>
        /// <param name="command">Received command from a certain source</param>
        /// <returns>A new or modified command with new data</returns>
        [CommandListener("MVM.TestService.HaalPolissenOpQueue")]
        public HaalPolissenOpCommand Handles(HaalPolissenOpCommand command)
        {
            command.Polisses = _context.Polissen.ToArray();
            return command;
        }
        
        /// <summary>
        /// Handle a Command and immediately throw an exception without any reason
        /// </summary>
        /// <param name="polissenOpCommand">Command</param>
        /// <returns>An exception</returns>
        [CommandListener("exception.test")]
        public ExceptionCommand Handles(ExceptionCommand polissenOpCommand)
        {
            throw new MysteriousException("Something mysterious happened!");
        }
    }
}