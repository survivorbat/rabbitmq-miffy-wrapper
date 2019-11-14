using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Minor.Miffy.TestBus
{
    public class TestCommandSender : ICommandSender
    {
        /// <summary>
        /// Context
        /// </summary>
        private readonly TestBusContext _context;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<TestCommandSender> _logger;

        /// <summary>
        /// Instantiate a command sender with a context
        /// </summary>
        public TestCommandSender(TestBusContext context)
        {
            _context = context;
            _logger = MiffyLoggerFactory.CreateInstance<TestCommandSender>();
        }

        /// <summary>
        /// Send a command asynchronously
        /// </summary>
        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            throw new System.NotImplementedException();
        }
    }
}