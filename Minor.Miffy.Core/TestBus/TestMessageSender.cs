using System.Collections.Generic;
using System.Linq;

namespace Minor.Miffy.TestBus
{
    public class TestMessageSender : IMessageSender
    {
        /// <summary>
        /// Testbus context
        /// </summary>
        private readonly TestBusContext _context;
        
        /// <summary>
        /// Testbuscontext to send the message to
        /// </summary>
        public TestMessageSender(TestBusContext context) => _context = context;

        /// <summary>
        /// Send a message to the in-memory bus
        /// </summary>
        public void SendMessage(EventMessage message)
        {
            IEnumerable<TestBusKey> matchingTopics = _context.DataQueues.Keys.Where(key => key.TopicPattern.IsMatch(message.Topic));
            
            foreach (var key in matchingTopics)
            {
                _context.DataQueues[key].AutoResetEvent.Set();
                _context.DataQueues[key].Queue.Enqueue(message);
            }
        }
    }
}