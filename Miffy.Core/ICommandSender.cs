using System.Threading.Tasks;

namespace Miffy
{
    public interface ICommandSender
    {
        Task<CommandMessage> SendCommandAsync(CommandMessage request);
    }
}
