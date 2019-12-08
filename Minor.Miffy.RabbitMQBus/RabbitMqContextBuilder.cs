using System;
using Microsoft.Extensions.Logging;
using Minor.Miffy.RabbitMQBus.Constants;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus
{
    public class RabbitMqContextBuilder
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<RabbitMqContextBuilder> _logger;

        /// <summary>
        /// Connection string
        /// </summary>
        public Uri ConnectionString { get; private set; }

        /// <summary>
        /// Exchange name
        /// </summary>
        public string ExchangeName { get; private set; }

        /// <summary>
        /// Initialize a builder with a logger
        /// </summary>
        public RabbitMqContextBuilder()
        {
            _logger = RabbitMqLoggerFactory.CreateInstance<RabbitMqContextBuilder>();
        }

        /// <summary>
        /// Set up an exchange name
        /// </summary>
        public RabbitMqContextBuilder WithExchange(string exchangeName)
        {
            _logger.LogTrace($"Setting exchange name as {exchangeName}");
            ExchangeName = exchangeName;
            return this;
        }

        /// <summary>
        /// Set the connection string to the broker
        ///
        /// Not logging the url since it might leak secrets
        /// </summary>
        public RabbitMqContextBuilder WithConnectionString(string url)
        {
            _logger.LogTrace($"Setting connection string");
            ConnectionString = new Uri(url);
            return this;
        }

        /// <summary>
        /// Initialize the connection to the broker using environment variables
        /// </summary>
        public RabbitMqContextBuilder ReadFromEnvironmentVariables()
        {
            string url = Environment.GetEnvironmentVariable(EnvVarNames.BrokerConnectionString) ??
                           throw new BusConfigurationException($"{EnvVarNames.BrokerConnectionString} env variable not set");
            ExchangeName = Environment.GetEnvironmentVariable(EnvVarNames.BrokerExchangeName) ??
                           throw new BusConfigurationException($"{EnvVarNames.BrokerExchangeName} env variable not set");

            _logger.LogDebug($"Setting exchange name as {ExchangeName} and setting connection string");

            ConnectionString = new Uri(url);
            return this;
        }

        /// <summary>
        /// Creates a context with
        ///  - an opened connection (based on the URI)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        /// <param name="connectionFactory">Use a custom connection factory</param>
        public IBusContext<IConnection> CreateContext(IConnectionFactory connectionFactory)
        {
            connectionFactory.Uri = ConnectionString;

            IConnection connection = connectionFactory.CreateConnection();

            using (var channel = connection.CreateModel())
            {
                _logger.LogDebug($"Declaring exchange {ExchangeName}");
                channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic);
            }

            return new RabbitMqBusContext(connection, ExchangeName);
        }

        /// <summary>
        /// Creates a context with
        ///  - an opened connection (based on the URI)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        public IBusContext<IConnection> CreateContext()
        {
            return CreateContext(new ConnectionFactory());
        }
    }
}
