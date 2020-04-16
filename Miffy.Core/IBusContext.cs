using System;
using System.Collections.Generic;

namespace Miffy
{
    public interface IBusContext<TConnection> : IDisposable
    {
        TConnection Connection { get; }
        string ExchangeName { get; }

        IMessageSender CreateMessageSender();
        IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions);
        
        ICommandSender CreateCommandSender();
        ICommandReceiver CreateCommandReceiver(string queueName);
    }
}
