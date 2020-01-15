using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Host.HostEventArgs;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Host
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IMicroserviceHost
    {
        /// <summary>
        /// Connection context
        /// </summary>
        public IBusContext<IConnection> Context { get; }

        /// <summary>
        /// Handler that fires every time a message comes in
        /// </summary>
        public event MessageReceivedEventHandler EventMessageReceived;

        /// <summary>
        /// Handler that fires once the host starts listening
        /// </summary>
        public event HostStartedEventHandler HostStarted;

        /// <summary>
        /// Handler that fires every time a host is paused
        /// </summary>
        public event HostPausedEventHandler HostPaused;

        /// <summary>
        /// Handler that fires every time a host is resumed
        /// </summary>
        public event HostResumedEventHandler HostResumed;

        /// <summary>
        /// Indicate whether the host is paused or not
        /// </summary>
        public bool IsPaused { get; protected set; }

        /// <summary>
        /// Indicatee whether the host is listening or not
        /// </summary>
        public bool IsListening { get; protected set; }

        /// <summary>
        /// Queuename that this service will listen to
        /// </summary>
        public string QueueName { get; protected set; }

        /// <summary>
        /// A list of queues that have a list of associated topics with handlers.
        /// </summary>
        public IEnumerable<MicroserviceListener> Listeners { get; }

        /// <summary>
        /// A list of queues that are used to receive commands
        /// </summary>
        public IEnumerable<MicroserviceCommandListener> CommandListeners { get; }

        /// <summary>
        /// List of message receivers
        /// </summary>
        protected IMessageReceiver MessageReceiver { get; set; }

        /// <summary>
        /// A list of topics used in all the listeners
        /// </summary>
        protected IEnumerable<string> Topics { get; }

        /// <summary>
        /// List of command receivers
        /// </summary>
        protected List<ICommandReceiver> CommandReceivers { get; } = new List<ICommandReceiver>();

        /// <summary>
        /// A logger factory to log the start
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Create a new Microservice host
        /// </summary>
        /// <param name="connection">IBusContext for the connection with the message bus</param>
        /// <param name="listeners">All the listeners</param>
        /// <param name="commandListeners">All the command listeners</param>
        /// <param name="queueName">Name of the queue </param>
        /// <param name="logger">Logging instance</param>
        public MicroserviceHost(
            IBusContext<IConnection> connection,
            IEnumerable<MicroserviceListener> listeners,
            IEnumerable<MicroserviceCommandListener> commandListeners,
            string queueName,
            ILogger<MicroserviceHost> logger)
        {
            Context = connection;
            Listeners = listeners;
            CommandListeners = commandListeners;
            QueueName = queueName;
            Logger = logger;

            Topics = Listeners.SelectMany(e => e.TopicExpressions)
                    .Distinct();
        }

        /// <summary>
        /// Start listening for events
        /// </summary>
        public virtual void Start()
        {
            if (IsListening)
            {
                throw new BusConfigurationException("Attempted to start the MicroserviceHost, but it has already started.");
            }

            OnHostStarted();
            IsListening = true;

            foreach (MicroserviceCommandListener callback in CommandListeners)
            {
                Logger.LogInformation($"Registering command queue {callback.Queue}");

                ICommandReceiver receiver = Context.CreateCommandReceiver(callback.Queue);
                receiver.DeclareCommandQueue();
                receiver.StartReceivingCommands(callback.Callback);
                CommandReceivers.Add(receiver);
            }

            MessageReceiver = Context.CreateMessageReceiver(QueueName, Topics);
            MessageReceiver.StartReceivingMessages();
            MessageReceiver.StartHandlingMessages(eventMessage =>
            {
                OnMessageReceived(eventMessage);
                foreach (MicroserviceListener microserviceListener in Listeners)
                {
                    foreach (Regex expression in microserviceListener.TopicRegularExpressions)
                    {
                        if (expression.IsMatch(eventMessage.Topic))
                        {
                            microserviceListener.Callback.Invoke(eventMessage);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Pause normal operations
        /// </summary>
        public void Pause()
        {
            if (!IsListening)
            {
                throw new BusConfigurationException("Attempted to pause the MicroserviceHost, but host has not been started.");
            }

            if (IsPaused)
            {
                throw new BusConfigurationException("Attempted to pause the MicroserviceHost, but it was already paused.");
            }

            IsPaused = true;
            MessageReceiver.Pause();
            CommandReceivers.ForEach(e => e.Pause());
            OnHostPaused();
        }

        /// <summary>
        /// Resume normal operations
        /// </summary>
        public void Resume()
        {
            if (!IsListening)
            {
                throw new BusConfigurationException("Attempted to resume the MicroserviceHost, but host has not been started.");
            }

            if (!IsPaused)
            {
                throw new BusConfigurationException("Attempted to resume the MicroserviceHost, but it wasn't paused.");
            }

            IsPaused = false;
            MessageReceiver.Resume();
            CommandReceivers.ForEach(e => e.Resume());
            OnHostResumed();
        }

        /// <summary>
        /// Dispose of the context
        /// </summary>
        public virtual void Dispose()
        {
            Logger.LogDebug("Disposing of message receiver");
            MessageReceiver?.Dispose();

            Logger.LogDebug("Disposing each command receiver");
            CommandReceivers.ForEach(e => e.Dispose());

            Logger.LogDebug("Disposing bus context");
            Context.Dispose();
        }

        /// <summary>
        /// Event invocator for receiving a message
        /// </summary>
        protected virtual void OnMessageReceived(EventMessage eventmessage)
        {
            EventMessageReceivedEventArgs args = new EventMessageReceivedEventArgs
            {
                QueueName = QueueName,
                TopicExpressions = Topics
            };

            EventMessageReceived?.Invoke(eventmessage, args);
        }

        /// <summary>
        /// Event invocator for when a host is paused
        /// </summary>
        protected virtual void OnHostPaused()
        {
            HostPaused?.Invoke(this, new HostPausedEventArgs());
        }

        /// <summary>
        /// Event invocator for when a host is resumed
        /// </summary>
        protected virtual void OnHostResumed()
        {
            HostResumed?.Invoke(this, new HostResumedEventArgs());
        }

        /// <summary>
        /// Event invocator for when a host is started
        /// </summary>
        protected virtual void OnHostStarted()
        {
            HostStarted?.Invoke(this, new HostStartedEventArgs());
        }
    }
}
