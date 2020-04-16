using System;
using System.Threading.Tasks;

namespace Miffy.MicroServices.Events
{
    public interface IEventPublisher
    {
        void Publish(DomainEvent domainEvent);
        void Publish(long timeStamp, string topic, Guid correlationId, string eventType, string body);

        Task PublishAsync(DomainEvent domainEvent);
        Task PublishAsync(long timeStamp, string topic, Guid correlationId, string eventType, string body);
    }
}
