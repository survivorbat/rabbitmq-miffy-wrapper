using System;
using RabbitMQ.Client;

namespace Minor.Miffy.Microservices.Test.Integration.Integration
{
    internal static class RabbitMqCleanUp
    {
        /// <summary>
        /// Delete an entire exchange to clean up tests
        /// </summary>
        /// <param name="exchange">Name of the exchange to delete</param>
        /// <param name="connectionString">Bus to connect to</param>
        internal static void DeleteExchange(string exchange, string connectionString)
        {
            ConnectionFactory factory = new ConnectionFactory {Uri = new Uri(connectionString)};

            using IConnection connection = factory.CreateConnection();
            using IModel model = connection.CreateModel();

            model.ExchangeDelete(exchange);
        }

        /// <summary>
        /// Delete a queue
        /// </summary>
        /// <param name="queueName">Name of the queue to delete</param>
        /// <param name="connectionString">Bus to connect to</param>
        internal static void DeleteQueue(string queueName, string connectionString)
        {
            ConnectionFactory factory = new ConnectionFactory {Uri = new Uri(connectionString)};

            using IConnection connection = factory.CreateConnection();
            using IModel model = connection.CreateModel();

            model.QueueDelete(queueName);
        }
    }
}
