using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqBusContextTest
    {
        [TestMethod]
        [DataRow("test.exchange")]
        [DataRow("exchangeTest")]
        public void ExchangeNameIsProperlySet(string exchangeName)
        {
            // Arrange
            var connection = new Mock<IConnection>();
            
            // Act
            var context = new RabbitMqBusContext(connection.Object, exchangeName);

            // Assert
            Assert.AreEqual(exchangeName, context.ExchangeName);
        }
        
        [TestMethod]
        public void ConnectionIsProperlySet()
        {
            // Arrange
            var connection = new Mock<IConnection>();
            
            // Act
            var context = new RabbitMqBusContext(connection.Object, "test");

            // Assert
            Assert.AreSame(connection.Object, context.Connection);
        }

        [TestMethod]
        public void DiposeCallsDisposeOnConnection()
        {
            // Arrange
            var connection = new Mock<IConnection>();
            var context = new RabbitMqBusContext(connection.Object, "test");
            
            // Act
            context.Dispose();
            
            // Assert
            connection.Verify(e => e.Dispose());
        }
        
        [TestMethod]
        public void CreateMessageReceiverReturnsReceiver()
        {
            // Arrange
            var connection = new Mock<IConnection>();
            var context = new RabbitMqBusContext(connection.Object, "test.exchange");
            
            // Act
            var result = context.CreateMessageReceiver("test", new string[0]);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RabbitMqMessageReceiver));
        }
        
        [TestMethod]
        public void CreateMessageSenderReturnsPublisher()
        {
            // Arrange
            var connection = new Mock<IConnection>();
            var context = new RabbitMqBusContext(connection.Object, "test.exchange");
            
            // Act
            var result = context.CreateMessageSender();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RabbitMqMessagePublisher));
        }
        
        [TestMethod]
        public void CreateCommandSenderReturnsPublisher()
        {
            // Arrange
            var connection = new Mock<IConnection>();
            var context = new RabbitMqBusContext(connection.Object, "test.exchange");
            
            // Act
            var result = context.CreateCommandSender();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RabbitMqCommandSender));
        }
        
        [TestMethod]
        public void CreateCommandReceiverReturnsReceiver()
        {
            // Arrange
            var connection = new Mock<IConnection>();
            var context = new RabbitMqBusContext(connection.Object, "test.exchange");
            
            // Act
            var result = context.CreateCommandReceiver("test");

            // Assert
            Assert.IsInstanceOfType(result, typeof(RabbitMqCommandReceiver));
        }
    }
}