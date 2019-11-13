using System;
using System.Collections.Generic;
using System.Text;
using Minor.Miffy.RabbitMQBus.Constants;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqContextBuilder
    {
        public Uri ConnectionString { get; private set; }
        public string ExchangeName { get; private set; }
        
        public RabbitMqContextBuilder WithExchange(string exchangeName)
        {
            ExchangeName = exchangeName;
            return this;
        }

        public RabbitMqContextBuilder WithConnectionString(string url)
        {
            ConnectionString = new Uri(url);
            return this;
        }

        public RabbitMqContextBuilder ReadFromEnvironmentVariables()
        {
            string url = Environment.GetEnvironmentVariable(EnvVarNames.BrokerConnectionString) ?? 
                           throw new BusConfigurationException($"{EnvVarNames.BrokerConnectionString} env variable not set");
            ExchangeName = Environment.GetEnvironmentVariable(EnvVarNames.BrokerExchangeName) ?? 
                           throw new BusConfigurationException($"{EnvVarNames.BrokerExchangeName} env variable not set");

            ConnectionString = new Uri(url);
            return this;
        }

        /// <summary>
        /// Creates a context with 
        ///  - an opened connection (based on HostName, Port, UserName and Password)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        public IBusContext<IConnection> CreateContext()
        {
            ConnectionFactory factory = new ConnectionFactory { Uri = ConnectionString };
            IConnection connection = factory.CreateConnection();
            
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic);
            }
            
            return new RabbitMqBusContext(connection, ExchangeName);
        }
    }

}
