using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqContextBuilder
    {
        private Uri _connectionString;
        private string _exchangeName;
        
        public RabbitMqContextBuilder WithExchange(string exchangeName)
        {
            _exchangeName = exchangeName;
            return this;
        }

        public RabbitMqContextBuilder WithConnectionString(string url)
        {
            _connectionString = new Uri(url);
            return this;
        }

        public RabbitMqContextBuilder ReadFromEnvironmentVariables()
        {
            string url = Environment.GetEnvironmentVariable("BROKER_CONNECTION_STRING");
            _exchangeName = Environment.GetEnvironmentVariable("BROKER_EXCHANGE_NAME");
            
            _connectionString = new Uri(url);
            return this;
        }

        /// <summary>
        /// Creates a context with 
        ///  - an opened connection (based on HostName, Port, UserName and Password)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        /// <returns></returns>
        public IBusContext<IConnection> CreateContext()
        {
            ConnectionFactory factory = new ConnectionFactory { Uri = _connectionString };
            IConnection connection = factory.CreateConnection();
            
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
            }
            
            return new RabbitMqBusContext(connection, _exchangeName);
        }
    }

}
