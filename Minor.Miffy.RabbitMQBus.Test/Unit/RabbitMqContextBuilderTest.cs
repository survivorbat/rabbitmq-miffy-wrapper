using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.RabbitMQBus.Constants;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqContextBuilderTest
    {
        [TestMethod]
        [DataRow("TestExchange")]
        [DataRow("MVM.Blackjack.Exchange")]
        public void WithExchangeSetsExchangeName(string exchange)
        {
            // Arrange
            var builder = new RabbitMqContextBuilder();

            // Act
            builder.WithExchange(exchange);

            // Assert
            Assert.AreEqual(exchange, builder.ExchangeName);
        }

        [TestMethod]
        [DataRow("amqp://test:test@infosupport.net")]
        [DataRow("amqp://guest:guest@localhost")]
        public void WithConnectionStringSetsConnectionStringUri(string connectionString)
        {
            // Arrange
            var builder = new RabbitMqContextBuilder();

            // Act
            builder.WithConnectionString(connectionString);

            // Assert
            Assert.AreEqual(new Uri(connectionString), builder.ConnectionString);
        }

        [TestMethod]
        [DataRow("amqp://guest:guest@localhost", "testExchange")]
        [DataRow("amqp://test:test@infosupport.com", "BlackJack")]
        public void ReadFromEnvironmentVariablesWorks(string connectionString, string exchangeName)
        {
            // Arrange
            var builder = new RabbitMqContextBuilder();

            Environment.SetEnvironmentVariable(EnvVarNames.BrokerConnectionString, connectionString);
            Environment.SetEnvironmentVariable(EnvVarNames.BrokerExchangeName, exchangeName);

            // Act
            builder.ReadFromEnvironmentVariables();

            // Assert
            Assert.AreEqual(new Uri(connectionString), builder.ConnectionString);
            Assert.AreEqual(exchangeName, builder.ExchangeName);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariablesThrowsExceptionOnMissingConnectionString()
        {
            // Arrange
            var builder = new RabbitMqContextBuilder();

            Environment.SetEnvironmentVariable(EnvVarNames.BrokerExchangeName, "testExchange");
            Environment.SetEnvironmentVariable(EnvVarNames.BrokerConnectionString, null);

            // Act
            void Act() => builder.ReadFromEnvironmentVariables();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual($"{EnvVarNames.BrokerConnectionString} env variable not set", exception.Message);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariablesThrowsExceptionOnMissingExchangeName()
        {
            // Arrange
            var builder = new RabbitMqContextBuilder();

            Environment.SetEnvironmentVariable(EnvVarNames.BrokerConnectionString, "amqp://guest:guest@localhost");
            Environment.SetEnvironmentVariable(EnvVarNames.BrokerExchangeName, null);

            // Act
            void Act() => builder.ReadFromEnvironmentVariables();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual($"{EnvVarNames.BrokerExchangeName} env variable not set", exception.Message);
        }

        [TestMethod]
        [DataRow("amqp://test:test@localhost/", "test.exchange")]
        [DataRow("amqp://guest:guest@localhost/", "guest.exchange")]
        [DataRow("amqp://admin:admin@localhost/", "AdminEschange")]
        public void CreateContextCreatesProperContext(string connectionUrl, string exchangeName)
        {
            // Arrange
            var builder = new RabbitMqContextBuilder();

            var connectionFactoryMock = new Mock<IConnectionFactory>();

            connectionFactoryMock.SetupProperty(e => e.Uri, new Uri("amqp://localhost"));

            var connectionMock = new Mock<IConnection>();
            connectionFactoryMock.Setup(e => e.CreateConnection())
                .Returns(connectionMock.Object);

            var modelMock = new Mock<IModel>();
            connectionMock.Setup(e => e.CreateModel())
                .Returns(modelMock.Object);

            // Act
            builder.WithConnectionString(connectionUrl)
                .WithExchange(exchangeName)
                .CreateContext(connectionFactoryMock.Object);

            // Assert
            Assert.AreEqual(connectionUrl, connectionFactoryMock.Object.Uri.ToString());
            modelMock.Verify(e =>
                e.ExchangeDeclare(exchangeName, ExchangeType.Topic, false, false, null));
        }
    }
}
