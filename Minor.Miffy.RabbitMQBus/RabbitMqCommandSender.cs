using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        public async Task<CommandMessage> SendCommandAsync(CommandMessage request) =>
            await Task.Run(() =>
            {
                using var channel = _context.Connection.CreateModel();
                var replyQueue = channel.QueueDeclare().QueueName;

                var consumer = new EventingBasicConsumer(channel);

                var props = channel.CreateBasicProperties();
                props.CorrelationId = request.CorrelationId.ToString();
                props.ReplyTo = replyQueue;
                props.Timestamp = new AmqpTimestamp(request.Timestamp);

                ManualResetEvent resetEvent = new ManualResetEvent(false);
                CommandMessage result = null;

                consumer.Received += (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId != request.CorrelationId.ToString()) return;

                    var response = Encoding.Unicode.GetString(ea.Body);
                    result = JsonConvert.DeserializeObject<CommandMessage>(response);
                    resetEvent.Set();
                };
                
                channel.BasicPublish(_context.ExchangeName, request.DestinationQueue, props, request.Body);
                channel.BasicConsume(replyQueue, true, consumer);
                
                resetEvent.WaitOne();

                return result;
            });

        /// <summary>
        /// Dispose of the model
        /// </summary>
        public void Dispose() { }
    }
}