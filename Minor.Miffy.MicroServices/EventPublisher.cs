using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices
{
    public class EventPublisher : IEventPublisher
    {
        /// <summary>
        /// Sender to send a message through the bus
        /// </summary>
        private readonly IMessageSender _sender;
        
        /// <summary>
        /// Create a publisher and initialize a sender
        /// </summary>
        public EventPublisher(IBusContext<IConnection> context) => _sender = context.CreateMessageSender();

        /// <summary>
        /// Publish a domain event
        /// </summary>
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
