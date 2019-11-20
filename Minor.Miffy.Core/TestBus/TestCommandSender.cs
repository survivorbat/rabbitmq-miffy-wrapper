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
        /// Instantiate a command sender with a context
        /// </summary>
        public TestCommandSender(TestBusContext context) => _context = context;

        /// <summary>
        /// Send a command asynchronously
        /// </summary>
        public Task<CommandMessage> SendCommandAsync(CommandMessage request) =>
            Task.Run(() =>
            {
                _context.CommandQueues[request.ReplyQueue] = new TestBusQueueWrapper<CommandMessage>();
                
                _context.CommandQueues[request.DestinationQueue].Queue.Enqueue(request);
                _context.CommandQueues[request.DestinationQueue].AutoResetEvent.Set();

                _context.CommandQueues[request.ReplyQueue].AutoResetEvent.WaitOne();
                _context.CommandQueues[request.ReplyQueue].Queue.TryDequeue(out var output);
                return output;
            });
    }
}