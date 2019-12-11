using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Miffy.RabbitMQBus
{
    /// <summary>
    /// Low-level implementation of receiving messages from a bus
    ///
    /// If you want to receive messages in a listener, consider using EventListeners in the microservice package
    /// </summary>
    public class RabbitMqMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// Dispose of both the model and the context
        /// </summary>
        public virtual void Dispose()
        {
            Model.Dispose();
        }

        /// <summary>
        /// Model used to listen to broker
        /// </summary>
        protected readonly IModel Model;

        /// <summary>
        /// Context that is connected to the broker
        /// </summary>
        protected readonly IBusContext<IConnection> Context;

        /// <summary>
        /// Logger to log received messages
        /// </summary>
        protected readonly ILogger<RabbitMqMessageReceiver> Logger;

        /// <summary>
        /// Whether the current message receiver is listening to the broker
        /// </summary>
        protected bool IsListening { get; set; }

        /// <summary>
        /// Initialize a message receiver with a context, queue name and topic filters
        /// </summary>
        public RabbitMqMessageReceiver(IBusContext<IConnection> context, string queueName, IEnumerable<string> topicFilters)
        {
            Context = context;
            Model = Context.Connection.CreateModel();
            QueueName = queueName;
            TopicFilters = topicFilters;
            Logger = RabbitMqLoggerFactory.CreateInstance<RabbitMqMessageReceiver>();
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
        public virtual void StartReceivingMessages()
        {
            if (IsListening)
            {
                throw new BusConfigurationException("Receiver is already listening to events!");
            }

            Logger.LogDebug($"Declaring queue {QueueName} with {TopicFilters.Count()} topic expressions");
            Model.QueueDeclare(QueueName, true, false, false);
            foreach (string topicExpression in TopicFilters)
            {
                Model.QueueBind(QueueName, Context.ExchangeName, topicExpression);
            }

            IsListening = true;
        }

        /// <summary>
        /// Start consuming messages from the queue
        /// </summary>
        public virtual void StartHandlingMessages(EventMessageReceivedCallback callback)
        {
            if (!IsListening)
            {
                throw new BusConfigurationException("Receiver is not listening to events");
            }

            EventingBasicConsumer consumer = new EventingBasicConsumer(Model);
            consumer.Received += (model, args) =>
            {
                Logger.LogInformation($"Received event with id {args.BasicProperties.CorrelationId} " +
                                       $"of type {args.BasicProperties.Type} " +
                                       $"with topic {args.RoutingKey}");

                EventMessage eventMessage = new EventMessage
                {
                    Body = args.Body,
                    Timestamp = args.BasicProperties.Timestamp.UnixTime,
                    Topic = args.RoutingKey,
                    EventType = args.BasicProperties.Type,
                    CorrelationId = new Guid(args.BasicProperties.CorrelationId)
                };

                callback(eventMessage);
            };

            Logger.LogDebug($"Start consuming queue {QueueName}");
            Model.BasicConsume(QueueName, true, "", false, false, null, consumer);
        }
    }
}
