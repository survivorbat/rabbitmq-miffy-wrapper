using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Events
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
            loggerFactory ??= new NullLoggerFactory();
            _logger = loggerFactory.CreateLogger<EventPublisher>();
        }

        /// <summary>
        /// Publish a domain event
        /// </summary>
        public void Publish(DomainEvent domainEvent)
        {
            _logger.LogTrace($"Publishing domain event with type {domainEvent.Type} and ID {domainEvent.Id}");

            string json = JsonConvert.SerializeObject(domainEvent);

            _logger.LogDebug($"Publishing domain event {domainEvent.Id} with body: {json}");

            EventMessage message = new EventMessage
            {
                Timestamp = domainEvent.Timestamp,
                Topic = domainEvent.Topic,
                CorrelationId = domainEvent.Id,
                EventType = domainEvent.Type,
                Body = Encoding.Unicode.GetBytes(json)
            };

            _sender.SendMessage(message);
        }

        /// <summary>
        /// Publish a raw message to the bus with explicit values
        ///
        /// In case you want to publish a 'normal' event you're strongly encouraged to use the
        /// DomainEvent variant.
        /// </summary>
        public void Publish(long timeStamp, string topic, Guid correlationId, string eventType, string body)
        {
            _logger.LogTrace($"Publishing domain event with type {eventType} and ID {correlationId.ToString()} and body {body}");

            EventMessage message = new EventMessage
            {
                Timestamp = timeStamp,
                Topic = topic,
                CorrelationId = correlationId,
                EventType = eventType,
                Body = Encoding.Unicode.GetBytes(body)
            };

            _sender.SendMessage(message);
        }
    }
}
