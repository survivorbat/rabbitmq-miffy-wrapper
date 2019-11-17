using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Minor.Miffy.MicroServices.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        /// <summary>
        /// Sender to send a message through the bus
        /// </summary>
        private readonly ICommandSender _sender;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<EventPublisher> _logger;

        /// <summary>
        /// Create a publisher and initialize a sender
        /// </summary>
        public CommandPublisher(IBusContext<IConnection> context, ILoggerFactory loggerFactory = null)
        {
            _sender = context.CreateCommandSender();
            loggerFactory = loggerFactory ?? new NullLoggerFactory();
            _logger = loggerFactory.CreateLogger<EventPublisher>();
        }

        /// <summary>
        /// Publish a domain event
        /// </summary>
        public async Task<T> PublishAsync<T>(DomainCommand domainEvent)
        {
            _logger.LogTrace($"Publishing domain command with type {domainEvent.GetType().Name} and ID {domainEvent.Id}");
            
            var json = JsonConvert.SerializeObject(domainEvent);
            
            _logger.LogDebug($"Publishing domain event {domainEvent.Id} with body: {json}");
            
            var message = new CommandMessage
            {
                Timestamp = domainEvent.Timestamp,
                CorrelationId = domainEvent.Id,
                EventType = domainEvent.GetType().Name,
                Body = Encoding.Unicode.GetBytes(json),
                DestinationQueue = domainEvent.DestinationQueue
            };
            
            var result = await _sender.SendCommandAsync(message);
            string jsonBody = Encoding.Unicode.GetString(result.Body);
            
            return (T) JsonConvert.DeserializeObject(jsonBody, typeof(T));
        }
    }
}
