using System.Threading.Tasks;

namespace Miffy.MicroServices.Commands
{
    public interface ICommandPublisher
    {
        Task<T> PublishAsync<T>(DomainCommand domainCommand);
    }
}
