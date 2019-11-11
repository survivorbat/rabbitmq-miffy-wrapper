using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMQContextBuilder
    {
        public RabbitMQContextBuilder WithExchange(string exchangeName)
        {
            // TODO
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder WithAddress(string hostName, int port)
        {
            // TODO
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder WithCredentials(string userName, string password)
        {
            // TODO
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder ReadFromEnvironmentVariables()
        {
            // TODO
            return this;    // for method chaining
        }

        /// <summary>
        /// Creates a context with 
        ///  - an opened connection (based on HostName, Port, UserName and Password)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        /// <returns></returns>
        public RabbitMQBusContext CreateContext()
        {
            throw new NotImplementedException();
            // TODO
        }
    }

}
