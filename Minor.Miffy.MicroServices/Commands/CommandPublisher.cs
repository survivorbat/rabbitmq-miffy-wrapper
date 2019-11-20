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
        public async Task<T> PublishAsync<T>(DomainCommand domainCommand)
        {
            _logger.LogTrace($"Publishing domain command with type {domainCommand.GetType().Name} and ID {domainCommand.Id}");
            
            var json = JsonConvert.SerializeObject(domainCommand);
            
            _logger.LogDebug($"Publishing domain event {domainCommand.Id} with body: {json}");
            
            var message = new CommandMessage
            {
                Timestamp = domainCommand.Timestamp,
                CorrelationId = domainCommand.Id,
                EventType = domainCommand.GetType().Name,
                Body = Encoding.Unicode.GetBytes(json),
                DestinationQueue = domainCommand.DestinationQueue
            };
            
            var result = await _sender.SendCommandAsync(message);
            string jsonBody = Encoding.Unicode.GetString(result.Body);
            
            return (T) JsonConvert.DeserializeObject(jsonBody, typeof(T));
        }
    }
}
