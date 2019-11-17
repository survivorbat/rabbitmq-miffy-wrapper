namespace Minor.Miffy.MicroServices.Events
{
    public interface IEventPublisher
    {
        void Publish(DomainEvent domainEvent);
    }
}
