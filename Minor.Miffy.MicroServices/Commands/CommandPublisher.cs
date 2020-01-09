using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        /// <summary>
        /// Sender to send a message through the bus
        /// </summary>
        protected readonly ICommandSender _sender;

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger<CommandPublisher> _logger;

        /// <summary>
        /// Create a publisher and initialize a sender
        /// </summary>
        public CommandPublisher(IBusContext<IConnection> context, ILoggerFactory loggerFactory = null)
        {
            _sender = context.CreateCommandSender();
            loggerFactory ??= new NullLoggerFactory();
            _logger = loggerFactory.CreateLogger<CommandPublisher>();
        }

        /// <summary>
        /// Publish a domain command with a specific return result
        /// </summary>
        public virtual async Task<TReturn> PublishAsync<TReturn>(DomainCommand domainCommand)
        {
            _logger.LogTrace(
                $"Publishing domain command with type {domainCommand.GetType().Name} and ID {domainCommand.Id}");

            var json = JsonConvert.SerializeObject(domainCommand);

            _logger.LogDebug($"Publishing domain command {domainCommand.Id} with body: {json}");

            var message = new CommandMessage
            {
                Timestamp = domainCommand.Timestamp,
                CorrelationId = domainCommand.Id,
                EventType = domainCommand.GetType().Name,
                Body = Encoding.Unicode.GetBytes(json),
                DestinationQueue = domainCommand.DestinationQueue
            };

            var result = await _sender.SendCommandAsync(message);

            try
            {
                string jsonBody = Encoding.Unicode.GetString(result?.Body);
                return (TReturn) JsonConvert.DeserializeObject(jsonBody, typeof(TReturn));
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(
                    $"Deserializing response from queue {domainCommand.DestinationQueue} ended with an ArgumentNullException");

                throw new DestinationQueueException(
                    $"ArgumentNullException was thrown, most likely because the destination queue {domainCommand.DestinationQueue} replied with an empty body.",
                    exception,
                    "Unknown",
                    domainCommand.DestinationQueue,
                    domainCommand.Id);
            }
        }
    }
}
