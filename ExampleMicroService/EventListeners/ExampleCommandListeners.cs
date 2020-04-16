using ExampleMicroService.Commands;
using ExampleMicroService.DAL;
using ExampleMicroService.Exceptions;
using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;

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
        public HaalPolissenOpCommandResult Handles(HaalPolissenOpCommand command)
        {
            return new HaalPolissenOpCommandResult
            {
                Polissen = _context.Polissen.ToArray()
            };
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
