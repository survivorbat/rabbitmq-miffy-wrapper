using System;
using System.Collections.Generic;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Host
{
    public interface IMicroserviceHost : IDisposable
    {
        bool IsPaused { get; }
        bool IsListening { get; }
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
}
