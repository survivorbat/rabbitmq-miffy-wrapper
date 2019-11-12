using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqMessageReceiver : IMessageReceiver
    {
        public void Dispose()
        {
            _model.Dispose();
            _context.Dispose();
        }

        private readonly IModel _model;
        private readonly IBusContext<IConnection> _context;

        public RabbitMqMessageReceiver(IBusContext<IConnection> context, string queueName, IEnumerable<string> topicFilters)
        {
            _context = context;
            _model = _context.Connection.CreateModel();
            QueueName = queueName;
            TopicFilters = topicFilters;
        }

        public string QueueName { get; }
        public IEnumerable<string> TopicFilters { get; }
        
        /// <summary>
        /// Create a queue and bind it to the exchange and the topic expression
        /// </summary>
        public void StartReceivingMessages()
        {
            _model.QueueDeclare(QueueName);
            foreach (var topicExpression in TopicFilters)
            {
                _model.QueueBind(QueueName, _context.ExchangeName, topicExpression);
            }
        }

        /// <summary>
        /// Start consuming messages from the queue
        /// </summary>
        public void StartHandlingMessages(EventMessageReceivedCallback callback)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(_model);
            consumer.Received += (model, args) =>
            {
                var eventMessage = new EventMessage
                {
                    Body = args.Body,
                    Timestamp = args.BasicProperties.Timestamp.UnixTime,
                    Topic = args.RoutingKey,
                    EventType = args.BasicProperties.Type
                };

                callback(eventMessage);
            };
            _model.BasicConsume(consumer, QueueName);
        }
    }
}