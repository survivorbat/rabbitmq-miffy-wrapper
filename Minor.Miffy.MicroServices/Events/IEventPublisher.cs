using System;

namespace Minor.Miffy.MicroServices.Events
{
    public interface IEventPublisher
    {
        void Publish(DomainEvent domainEvent);
        void Publish(long timeStamp, string topic, Guid correlationId, string eventType, string body);
    }
}
