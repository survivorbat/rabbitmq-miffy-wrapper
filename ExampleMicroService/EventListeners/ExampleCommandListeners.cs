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
        public ExampleCommandListener(PolisContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Receives a command, then returns a changed command
        /// </summary>
        [CommandListener("MVM.TestService.HaalPolissenOpQueue")]
        public HaalPolissenOpCommand Handles(HaalPolissenOpCommand command)
        {
            command.Polisses = _context.Polissen.ToArray();
            return command;
        }
        
        /// <summary>
        /// Handle a Command and immediately throw an exception without any reason
        /// </summary>
        [CommandListener("exception.test")]
        public ExceptionCommand Handles(ExceptionCommand polissenOpCommand)
        {
            throw new MysteriousException("Something mysterious happened!");
        }
    }
}