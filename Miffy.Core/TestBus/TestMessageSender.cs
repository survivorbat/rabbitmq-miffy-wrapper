using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Miffy.TestBus
{
    public class TestMessageSender : IMessageSender
    {
        /// <summary>
        /// Testbus context
        /// </summary>
        protected readonly TestBusContext _context;

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger<TestMessageSender> _logger;

        /// <summary>
        /// Testbuscontext to send the message to
        /// </summary>
        public TestMessageSender(TestBusContext context)
        {
            _context = context;
            _logger = MiffyLoggerFactory.CreateInstance<TestMessageSender>();
        }

        /// <summary>
        /// Send a message to the in-memory bus
        /// </summary>
        public virtual void SendMessage(EventMessage message)
        {
            IEnumerable<TestBusKey> matchingTopics = _context.DataQueues.Keys
                .Where(key => key.TopicPattern.IsMatch(message.Topic))
                .ToList();

            _logger.LogDebug($"Message {message.CorrelationId} received with matching topics {string.Join(", ", matchingTopics)}");

            foreach (TestBusKey key in matchingTopics)
            {
                _context.DataQueues[key].Queue.Enqueue(message);
                _context.DataQueues[key].AutoResetEvent.Set();
            }
        }
    }
}
