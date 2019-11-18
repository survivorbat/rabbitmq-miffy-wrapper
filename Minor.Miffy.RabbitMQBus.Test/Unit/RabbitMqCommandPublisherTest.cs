using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqCommandPublisherTest
    {
        [TestMethod]
        public void SendCommandCreatesChannel()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();

            contextMock.SetupGet(e => e.Connection)
                .Returns(connectionMock.Object);
            
            var sender = new RabbitMqCommandSender(contextMock.Object);
            
            var command = new CommandMessage();
            
            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(500);

            // Assert
            connectionMock.Verify(e => e.CreateModel(), Times.Once);
        }

        [TestMethod]
        public void SendCommandCallsQueueDeclare()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection)
                .Returns(connectionMock.Object);

            connectionMock.Setup(e => e.CreateModel())
                .Returns(modelMock.Object);
            
            var sender = new RabbitMqCommandSender(contextMock.Object);
            
            var command = new CommandMessage();
            
            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(500);

            // Assert
            modelMock.Verify(e => 
                e.QueueDeclare("", true, false, true, null), Times.Once());
        }
        
        [TestMethod]
        [DataRow("TestQueue", "TestExchange")]
        [DataRow("queue.test", "exchange.test")]
        public void SendCommandCallsQueueBindWithQueueName(string queueName, string exchangeName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.ExchangeName)
                .Returns(exchangeName);
            
            contextMock.SetupGet(e => e.Connection)
                .Returns(connectionMock.Object);

            connectionMock.Setup(e => e.CreateModel())
                .Returns(modelMock.Object);
            
            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));
            
            var sender = new RabbitMqCommandSender(contextMock.Object);
            
            var command = new CommandMessage();
            
            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(500);

            // Assert
            modelMock.Verify(e => e.QueueBind(queueName, exchangeName, queueName, null));
        }
        
        [TestMethod]
        [DataRow("TestQueue", "TestExchange")]
        [DataRow("queue.test", "exchange.test")]
        public void SendCommandCallsBasicConsume(string queueName, string exchangeName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.ExchangeName)
                .Returns(exchangeName);
            
            contextMock.SetupGet(e => e.Connection)
                .Returns(connectionMock.Object);

            connectionMock.Setup(e => e.CreateModel())
                .Returns(modelMock.Object);
            
            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));
            
            modelMock.Setup(e => e.QueueBind(queueName, exchangeName, queueName, null));

            modelMock.Setup(e => e.CreateBasicProperties())
                .Returns(new BasicProperties());

            var sender = new RabbitMqCommandSender(contextMock.Object);
            
            var command = new CommandMessage();
            
            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(500);

            // Assert
            modelMock.Verify(e => e.BasicConsume(queueName, 
                false, 
                It.IsAny<string>(), 
                false, 
                false, 
                null, 
                It.IsAny<EventingBasicConsumer>()));
        }
        
        [TestMethod]
        [DataRow("TestQueue", "TestExchange")]
        [DataRow("queue.test", "exchange.test")]
        public void SendCommandCallsBasicPublish(string queueName, string exchangeName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.ExchangeName)
                .Returns(exchangeName);
            
            contextMock.SetupGet(e => e.Connection)
                .Returns(connectionMock.Object);

            connectionMock.Setup(e => e.CreateModel())
                .Returns(modelMock.Object);
            
            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));
            
            modelMock.Setup(e => e.QueueBind(queueName, exchangeName, queueName, null));

            modelMock.Setup(e => e.CreateBasicProperties())
                .Returns(new BasicProperties());

            var sender = new RabbitMqCommandSender(contextMock.Object);
            
            var command = new CommandMessage
            {
                DestinationQueue = queueName
            };
            
            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(500);

            // Assert
            modelMock.Verify(e => e.BasicPublish(exchangeName, queueName, false, It.IsAny<BasicProperties>(), It.IsAny<byte[]>()));
        }
    }
}