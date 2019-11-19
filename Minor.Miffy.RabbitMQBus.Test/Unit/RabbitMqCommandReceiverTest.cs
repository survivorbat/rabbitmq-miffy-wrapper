using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqCommandReceiverTest
    {
        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("queue.test")]
        public void QueueNameIsProperlySet(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            
            // Act
            var receiver = new RabbitMqCommandReceiver(contextMock.Object, queueName);

            // Assert
            Assert.AreEqual(queueName, receiver.QueueName);
        }
        
        [TestMethod]
        public void CreateModelIsCalledInConstructor()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            
            // Act
            new RabbitMqCommandReceiver(contextMock.Object, "test.queue");

            // Assert
            connectionMock.Verify(e => e.CreateModel());
        }

        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("queue.test")]
        public void DeclareCommandQueueDeclaresQueue(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            
            var receiver = new RabbitMqCommandReceiver(contextMock.Object, queueName);
            
            // Act
            receiver.DeclareCommandQueue();

            // Assert
            modelMock.Verify(
                e => e.QueueDeclare(queueName, true, false, false, null));
        }
        
        [TestMethod]
        [DataRow("test.queue", "test.exchange")]
        [DataRow("test.queue", "exchange.test")]
        [DataRow("queue.test", "exchange.test")]
        [DataRow("queue.test", "test.exchange")]
        public void DeclareCommandQueueBindsQueue(string queueName, string exchangeName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            contextMock.SetupGet(e => e.ExchangeName).Returns(exchangeName);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            
            var receiver = new RabbitMqCommandReceiver(contextMock.Object, queueName);
            
            // Act
            receiver.DeclareCommandQueue();

            // Assert
            modelMock.Verify(e => e.QueueBind(queueName, exchangeName, queueName, null));
        }
        
        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("queue.test")]
        public void DeclareCommandQueueTwiceThrowsException(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            
            var receiver = new RabbitMqCommandReceiver(contextMock.Object, queueName);
            
            receiver.DeclareCommandQueue();

            // Act
            void Act() => receiver.DeclareCommandQueue();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual($"Queue {queueName} has already been declared!", exception.Message);
        }
        
        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("queue.test")]
        public void StartReceivingCommandsThrowsExceptionWhenNoQueueDeclared(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            
            var receiver = new RabbitMqCommandReceiver(contextMock.Object, queueName);

            // Act
            void Act() => receiver.StartReceivingCommands(e => new CommandMessage());

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual($"Queue {queueName} has not been declared yet", exception.Message);
        }

        [TestMethod]
        public void DisposeCallsDisposeOnModel()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var receiver = new RabbitMqCommandReceiver(contextMock.Object, "test.queue");
            
            // Act
            receiver.Dispose();
            
            // Assert
            modelMock.Verify(e => e.Dispose());
        }

        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("queue.test")]
        [DataRow("TestQueue")]
        public void StartReceivingCommandCallsBasicConsume(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var receiver = new RabbitMqCommandReceiver(contextMock.Object, queueName);
            
            receiver.DeclareCommandQueue();
            
            // Act
            receiver.StartReceivingCommands(e => new CommandMessage());
            
            // Assert
            modelMock.Verify(e => e.BasicConsume(queueName, false, "", false, false, null, It.IsAny<IBasicConsumer>()));
        }
    }
}