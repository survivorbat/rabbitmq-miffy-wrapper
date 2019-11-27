using System;
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
                string randomReplyQueueName = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                request.ReplyQueue = randomReplyQueueName;
                
                _context.CommandQueues[randomReplyQueueName] = new TestBusQueueWrapper<CommandMessage>();
                
                _context.CommandQueues[request.DestinationQueue].Queue.Enqueue(request);
                _context.CommandQueues[request.DestinationQueue].AutoResetEvent.Set();

                _context.CommandQueues[randomReplyQueueName].AutoResetEvent.WaitOne();
                _context.CommandQueues[randomReplyQueueName].Queue.TryDequeue(out CommandMessage output);
                return output;
            });
    }
}
