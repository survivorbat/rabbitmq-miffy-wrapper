using System;
using System.Collections.Generic;
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
        public static readonly int CommandTimeout = 8000;
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
        public Task<CommandMessage> SendCommandAsync(CommandMessage request) =>
            Task.Run(() =>
            {
                _logger.LogDebug($"Sending command with id {request.CorrelationId} to queue {request.DestinationQueue}");
                using var channel = _context.Connection.CreateModel();
                var replyQueue = channel.QueueDeclare(durable: true, exclusive: false).QueueName;
                channel.QueueBind(replyQueue, _context.ExchangeName, replyQueue);

                var consumer = new EventingBasicConsumer(channel);

                var props = channel.CreateBasicProperties();
                props.CorrelationId = request.CorrelationId.ToString();
                props.ReplyTo = replyQueue;
                request.ReplyQueue = replyQueue;
                props.Timestamp = new AmqpTimestamp(request.Timestamp);
                
                _logger.LogInformation($"Sending command with id {props.CorrelationId}, reply queue {props.ReplyTo} and destination {request.DestinationQueue}");

                ManualResetEvent resetEvent = new ManualResetEvent(false);
                CommandMessage result = null;

                consumer.Received += (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId != request.CorrelationId.ToString())
                    {
                        return;
                    }

                    _logger.LogInformation($"Received response with id {request.CorrelationId} on queue {replyQueue} from {request.DestinationQueue}");
                    
                    var response = Encoding.Unicode.GetString(ea.Body);

                    result = JsonConvert.DeserializeObject<CommandMessage>(response);

                    if (result.EventType == typeof(CommandError).Name)
                    {
                        result = JsonConvert.DeserializeObject<CommandError>(response);
                    }
                    
                    channel.BasicAck(ea.DeliveryTag, false);
                    resetEvent.Set();
                };
                
                channel.BasicConsume(replyQueue, false, "", false, false, null, consumer);
                channel.BasicPublish(_context.ExchangeName, request.DestinationQueue, true, props, request.Body);
                
                resetEvent.WaitOne(CommandTimeout);

                if (result is CommandError error)
                {
                    throw new DestinationQueueException(error.ExceptionMessage);
                }

                return result ?? throw new MessageTimeoutException($"No response received from queue {request.DestinationQueue} after {CommandTimeout}ms", CommandTimeout);
            });
    }
}