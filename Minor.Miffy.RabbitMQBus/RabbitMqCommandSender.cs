using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqCommandSender : ICommandSender
    {
        private readonly IBusContext<IConnection> _context;

        /// <summary>
        /// Dispose of the model
        /// </summary>
        public void Dispose()
        {
        }

        public RabbitMqCommandSender(IBusContext<IConnection> context) => _context = context;

        public async Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            using var model = _context.Connection.CreateModel();
            
            var resetHandler = new AutoResetEvent(false);
            
            // Declare reply queue ny the replyqueue name
            model.QueueDeclare(request.ReplyQueue);
            model.QueueBind(request.ReplyQueue, "", request.ReplyQueue);

            // Start consuming, waiting for a reply
            CommandMessage result = new CommandMessage();
            
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (sender, args) =>
            {
                result = new CommandMessage();
                resetHandler.Set();
            };

            model.BasicConsume(consumer, request.ReplyQueue);
            
            // Publish the command
            IBasicProperties basicProperties = new BasicProperties
            {
                Timestamp = new AmqpTimestamp(request.Timestamp),
                CorrelationId = request.CorrelationId.ToString(),
                ReplyTo = request.ReplyQueue,
                Type = request.EventType
            };
            
            model.BasicPublish("", request.DestinationQueue, basicProperties, request.Body);

            // Wait for reply to come in
            resetHandler.WaitOne();
            
            return result;
        }
    }
}