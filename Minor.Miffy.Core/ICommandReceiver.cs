using System;

namespace Minor.Miffy
{
    public interface ICommandReceiver : IDisposable
    {
        void DeclareCommandQueue();
        void StartReceivingCommands(CommandReceivedCallback callback);
    }

    public delegate CommandMessage CommandReceivedCallback(CommandMessage commandMessage);
}
