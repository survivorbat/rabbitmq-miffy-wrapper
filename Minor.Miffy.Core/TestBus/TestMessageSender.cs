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
            foreach (var (queue, topic) in _context.DataQueues.Keys)
            {
                if (topic == message.Topic)
                {
                    _context.DataQueues[(queue, topic)].AutoResetEvent.Set();
                    _context.DataQueues[(queue, topic)].Queue.Enqueue(message);
                }
            }
        }
    }
}