using System;
using System.Threading.Tasks;

namespace Minor.Miffy
{
    public interface ICommandSender
    {
        Task<CommandMessage> SendCommandAsync(CommandMessage request);
    }
}