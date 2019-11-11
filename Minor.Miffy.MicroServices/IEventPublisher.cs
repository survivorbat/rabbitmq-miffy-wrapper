using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Miffy.MicroServices
{
    public interface IEventPublisher
    {
        void Publish(DomainEvent domainEvent);
    }
}
