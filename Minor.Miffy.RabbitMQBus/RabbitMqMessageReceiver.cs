using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// Dispose of both the model and the context
        /// </summary>
        public void Dispose()
        {
            _model.Dispose();
            _context.Dispose();
        }

        /// <summary>
        /// Model used to listen to broker
        /// </summary>
        private readonly IModel _model;
        
        /// <summary>
        /// Context that is connected to the broker
        /// </summary>
        private readonly IBusContext<IConnection> _context;
        
        /// <summary>
        /// Whether the current message receiver is listening to the broker
        /// </summary>
        private bool _isListening;

        /// <summary>
        /// Initialize a message receiver with a context, queue name and topic filters
        /// </summary>
        public RabbitMqMessageReceiver(IBusContext<IConnection> context, string queueName, IEnumerable<string> topicFilters)
        {
            _context = context;
            _model = _context.Connection.CreateModel();
            QueueName = queueName;
            TopicFilters = topicFilters;
        }

        /// <summary>
        /// Name of the queue
        /// </summary>
        public string QueueName { get; }
        
        /// <summary>
        /// Topics that the queue is listening for
        /// </summary>
        public IEnumerable<string> TopicFilters { get; }
        
        /// <summary>
        /// Create a queue and bind it to the exchange and the topic expression
        /// </summary>
        public void StartReceivingMessages()
        {
            if (_isListening) throw new BusConfigurationException("Receiver is already listening to events!");
            
            _model.QueueDeclare(QueueName, true, false);
            foreach (var topicExpression in TopicFilters)
            {
                _model.QueueBind(QueueName, _context.ExchangeName, topicExpression);
            }

            _isListening = true;
        }

        /// <summary>
        /// TODO: Prevent this if not listening
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