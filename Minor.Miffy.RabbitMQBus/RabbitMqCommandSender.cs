using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqCommandSender : ICommandSender
    {
        /// <summary>
        /// Context
        /// </summary>
        private readonly IBusContext<IConnection> _context;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<RabbitMqCommandSender> _logger;

        /// <summary>
        /// Create a new sender with a provider context
        /// </summary>
        public RabbitMqCommandSender(IBusContext<IConnection> context)
        {
            _context = context;
            _logger = RabbitMqLoggerFactory.CreateInstance<RabbitMqCommandSender>();
        }

        /// <summary>
        /// Send a command asynchronously
        /// </summary>
        public async Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            return await Task.Run(() =>
            {
                return new CommandMessage();
            });
        }
        
        /// <summary>
        /// Dispose of the model
        /// </summary>
        public void Dispose() { }
    }
}