using System;
using System.Collections.Generic;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Host.HostEventArgs;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Host
{
    public interface IMicroserviceHost : IDisposable
    {
        event MessageReceivedEventHandler EventMessageReceived;
        event MessageHandledEventHandler EventMessageHandled;
        event HostStartedEventHandler HostStarted;
        event HostPausedEventHandler HostPaused;
        event HostResumedEventHandler HostResumed;

        bool IsPaused { get; }
        bool IsListening { get; }
        string QueueName { get; }
        IEnumerable<MicroserviceListener> Listeners { get; }
        IEnumerable<MicroserviceCommandListener> CommandListeners { get; }
        IBusContext<IConnection> Context { get; }

        /// <summary>
        /// Start listening for events
        /// </summary>
        void Start();

        /// <summary>
        /// Pause normal operations
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume normal operations
        /// </summary>
        void Resume();
    }

    public delegate void MessageReceivedEventHandler(EventMessage eventMessage, EventMessageReceivedEventArgs args);
    public delegate void MessageHandledEventHandler(EventMessage eventMessage, EventMessageHandledEventArgs args);
    public delegate void HostStartedEventHandler(IMicroserviceHost microserviceHost, HostStartedEventArgs args);
    public delegate void HostPausedEventHandler(IMicroserviceHost microserviceHost, HostPausedEventArgs args);
    public delegate void HostResumedEventHandler(IMicroserviceHost microserviceHost, HostResumedEventArgs args);
}
