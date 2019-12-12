using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Miffy.RabbitMQBus
{
    /// <summary>
    /// Low-level implementation of receiving commands from a bus
    ///
    /// If you want to receive commands in a listener, consider using Commandlisteners in the microservice package
    /// </summary>
    public class RabbitMqCommandReceiver : ICommandReceiver
    {
        protected const string CommandErrorType = "CommandError";

        /// <summary>
        /// Model used to send messages
        /// </summary>
        protected readonly IModel Model;

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger<RabbitMqCommandReceiver> Logger;

        /// <summary>
        /// Is queue declared?
        /// </summary>
        protected bool QueueDeclared { get; set; }

        /// <summary>
        /// Name of the queue for the commands
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Initialize a receiver with a context and queue name
        /// </summary>
        public RabbitMqCommandReceiver(IBusContext<IConnection> context, string queueName)
        {
            Model = context.Connection.CreateModel();
            QueueName = queueName;
            Logger = RabbitMqLoggerFactory.CreateInstance<RabbitMqCommandReceiver>();
        }

        /// <summary>
        /// Indicate whether the receiver is currently paused
        /// </summary>
        public bool IsPaused { get; protected set; }

        /// <summary>
        /// Declare the queue with the given queue name
        /// </summary>
        public virtual void DeclareCommandQueue()
        {
            if (QueueDeclared)
            {
                throw new BusConfigurationException($"Queue {QueueName} has already been declared!");
            }

            Logger.LogTrace($"Declaring command queue {QueueName}");
            Model.QueueDeclare(QueueName, true, false, false, null);
            QueueDeclared = true;
        }

        /// <summary>
        /// Start receiving commands and calling the callback
        /// </summary>
        public virtual void StartReceivingCommands(CommandReceivedCallback callback)
        {
            if (!QueueDeclared)
            {
                throw new BusConfigurationException($"Queue {QueueName} has not been declared yet");
            }

            Logger.LogTrace($"Start receiving commands on queue {QueueName}");

            EventingBasicConsumer consumer = new EventingBasicConsumer(Model);

            consumer.Received += (model, ea) =>
            {
                Logger.LogInformation(
                    $"Received command on {QueueName} with reply queue {ea.BasicProperties.ReplyTo}");
                IBasicProperties replyProps = Model.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                CommandMessage request = new CommandMessage
                {
                    Body = ea.Body,
                    Timestamp = ea.BasicProperties.Timestamp.UnixTime,
                    DestinationQueue = QueueName,
                    EventType = ea.BasicProperties.Type,
                    ReplyQueue = ea.BasicProperties.ReplyTo,
                    CorrelationId = Guid.Parse(ea.BasicProperties.CorrelationId)
                };

                CommandMessage response;
                try
                {
                    response = callback(request);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error occured while handling command, command id {ea.BasicProperties.CorrelationId} " +
                                     $"with exception {e.Message}");

                    response = new CommandError
                    {
                        Exception = e.InnerException ?? e,
                        EventType = CommandErrorType
                    };
                }

                string responseMessage = JsonConvert.SerializeObject(response);

                Logger.LogTrace($"Publishing command response with id {ea.BasicProperties.CorrelationId} " +
                                 $"reply queue {ea.BasicProperties.ReplyTo} and body {responseMessage}");

                Model.BasicPublish(
                    "",
                    ea.BasicProperties.ReplyTo,
                    replyProps,
                    Encoding.Unicode.GetBytes(responseMessage));

                Model.BasicAck(ea.DeliveryTag, false);
            };

            Model.BasicConsume(QueueName, false, "", false, false, null, consumer);
        }

        /// <summary>
        /// Pause the receiver
        /// </summary>
        public void Pause()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resume the receiver
        /// </summary>
        public void Resume()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispose of the created command
        /// </summary>
        public virtual void Dispose()
        {
            Model.Dispose();
        }
    }
}
