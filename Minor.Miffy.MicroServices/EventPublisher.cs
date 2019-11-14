using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        /// Logger
        /// </summary>
        private readonly ILogger<EventPublisher> _logger;

        /// <summary>
        /// Create a publisher and initialize a sender
        /// </summary>
        public EventPublisher(IBusContext<IConnection> context, ILoggerFactory loggerFactory = null)
        {
            _sender = context.CreateMessageSender();
            loggerFactory = loggerFactory ?? new NullLoggerFactory();
            _logger = loggerFactory.CreateLogger<EventPublisher>();
        }

        /// <summary>
        /// Publish a domain event
        /// </summary>
        public void Publish(DomainEvent domainEvent)
        {
            _logger.LogTrace($"Publishing domain event with type {domainEvent.GetType().Name} and ID {domainEvent.Id}");
            
            var json = JsonConvert.SerializeObject(domainEvent);
            
            _logger.LogDebug($"Publishing domain event {domainEvent.Id} with body: {json}");
            
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
