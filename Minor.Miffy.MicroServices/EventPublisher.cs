using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IBusContext<IConnection> _context;
        
        public EventPublisher(IBusContext<IConnection> context) => _context = context;

        public void Publish(DomainEvent domainEvent)
        {
            var json = JsonConvert.SerializeObject(domainEvent);
            
            EventMessage message = new EventMessage
            {
                Timestamp = domainEvent.Timestamp,
                Topic = domainEvent.Topic,
                CorrelationId = domainEvent.Id,
                EventType = domainEvent.GetType().Name,
                Body = Encoding.Unicode.GetBytes(json)
            };
            
            var sender = _context.CreateMessageSender();
            sender.SendMessage(message);
        }
    }
}
