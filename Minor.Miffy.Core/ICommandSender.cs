using System;
using System.Threading.Tasks;

namespace Minor.Miffy
{
    public interface ICommandSender : IDisposable
    {
        Task<CommandMessage> SendCommandAsync(CommandMessage request);
    }
}