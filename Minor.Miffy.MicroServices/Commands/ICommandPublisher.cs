using System.Threading.Tasks;

namespace Minor.Miffy.MicroServices.Commands
{
    public interface ICommandPublisher
    {
        Task<T> PublishAsync<T>(DomainCommand domainCommand);
        T Publish<T>(DomainCommand domainCommand);
    }
}
