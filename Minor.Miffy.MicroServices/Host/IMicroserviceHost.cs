using System;
using System.Collections.Generic;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Host
{
    public interface IMicroserviceHost : IDisposable
    {
        public bool IsPaused { get; }
        public bool IsListening { get; }
        public IEnumerable<MicroserviceListener> Listeners { get; }
        public IEnumerable<MicroserviceCommandListener> CommandListeners { get; }
        public IBusContext<IConnection> Context { get; }

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
