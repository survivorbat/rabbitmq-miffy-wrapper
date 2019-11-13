namespace Minor.Miffy.TestBus
{
    public class TestMessageSender : IMessageSender
    {
        private readonly TestBusContext _context;
        
        public TestMessageSender(TestBusContext context) => _context = context;

        /// <summary>
        /// Send a message to the in-memory bus
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(EventMessage message)
        {
            foreach (TestBusKey key in _context.DataQueues.Keys)
            {
                if (key.TopicName != message.Topic) continue;
                
                _context.DataQueues[key].AutoResetEvent.Set();
                _context.DataQueues[key].Queue.Enqueue(message);
            }
        }
    }
}