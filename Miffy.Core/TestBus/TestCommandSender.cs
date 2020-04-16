using System;
using System.Threading.Tasks;

namespace Miffy.TestBus
{
    public class TestCommandSender : ICommandSender
    {
        /// <summary>
        /// Context
        /// </summary>
        protected readonly TestBusContext Context;

        /// <summary>
        /// Instantiate a command sender with a context
        /// </summary>
        public TestCommandSender(TestBusContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Send a command asynchronously
        /// </summary>
        public virtual Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            return Task.Run(() =>
            {
                string randomReplyQueueName = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                request.ReplyQueue = randomReplyQueueName;

                Context.CommandQueues[randomReplyQueueName] = new TestBusQueueWrapper<CommandMessage>();

                Context.CommandQueues[request.DestinationQueue].Queue.Enqueue(request);
                Context.CommandQueues[request.DestinationQueue].AutoResetEvent.Set();

                Context.CommandQueues[randomReplyQueueName].AutoResetEvent.WaitOne();
                Context.CommandQueues[randomReplyQueueName].Queue.TryDequeue(out CommandMessage output);
                return output;
            });
        }
    }
}
