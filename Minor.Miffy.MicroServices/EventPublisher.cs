using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IMessageSender _sender;
        
        public EventPublisher(IBusContext<IConnection> context) => _sender = context.CreateMessageSender();

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
            
            _sender.SendMessage(message);
        }
    }
}
