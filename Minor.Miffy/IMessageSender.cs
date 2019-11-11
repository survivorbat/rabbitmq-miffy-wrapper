using System;

namespace Minor.Miffy
{
    public interface IMessageSender
    {
        void SendMessage(EventMessage message);
    }
}
