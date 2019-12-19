using System;
using System.Text;
using System.Threading.Tasks;
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
        protected readonly IMessageSender Sender;

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger<EventPublisher> Logger;

        /// <summary>
        /// Create a publisher and initialize a sender
        /// </summary>
        public EventPublisher(IBusContext<IConnection> context, ILoggerFactory loggerFactory = null)
        {
            Sender = context.CreateMessageSender();
            loggerFactory ??= new NullLoggerFactory();
            Logger = loggerFactory.CreateLogger<EventPublisher>();
        }

        /// <summary>
        /// Publish a domain event
        /// </summary>
        public virtual void Publish(DomainEvent domainEvent)
        {
            Logger.LogTrace($"Publishing domain event with type {domainEvent.Type} and ID {domainEvent.Id}");

            string json = JsonConvert.SerializeObject(domainEvent);

            Logger.LogDebug($"Publishing domain event {domainEvent.Id} with body: {json}");

            EventMessage message = new EventMessage
            {
                Timestamp = domainEvent.Timestamp,
                Topic = domainEvent.Topic,
                CorrelationId = domainEvent.Id,
                EventType = domainEvent.Type,
                Body = Encoding.Unicode.GetBytes(json)
            };

            Sender.SendMessage(message);
        }

        /// <summary>
        /// Publish a raw message to the bus with explicit values
        ///
        /// In case you want to publish a 'normal' event you're strongly encouraged to use the
        /// DomainEvent variant.
        /// </summary>
        public virtual void Publish(long timeStamp, string topic, Guid correlationId, string eventType, string body)
        {
            Logger.LogTrace($"Publishing domain event with type {eventType} and ID {correlationId.ToString()} and body {body}");

            EventMessage message = new EventMessage
            {
                Timestamp = timeStamp,
                Topic = topic,
                CorrelationId = correlationId,
                EventType = eventType,
                Body = Encoding.Unicode.GetBytes(body)
            };

            Sender.SendMessage(message);
        }

        /// <summary>
        /// Publish a new domain event asynchronously
        /// </summary>
        public async Task PublishAsync(DomainEvent domainEvent)
        {
            await Task.Run(() => Publish(domainEvent));
        }

        /// <summary>
        /// Publish a new domain event asynchronously
        /// </summary>
        public async Task PublishAsync(long timeStamp, string topic, Guid correlationId, string eventType, string body)
        {
            await Task.Run(() => Publish(timeStamp, topic, correlationId, eventType, body));
        }
    }
}
