using System;

namespace Minor.Miffy
{
    public interface ICommandReceiver : IDisposable
    {
        bool IsPaused { get; }

        void DeclareCommandQueue();
        void StartReceivingCommands(CommandReceivedCallback callback);

        void Pause();
        void Resume();
    }

    public delegate CommandMessage CommandReceivedCallback(CommandMessage commandMessage);
}
