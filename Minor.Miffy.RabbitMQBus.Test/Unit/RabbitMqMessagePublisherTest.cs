using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqMessagePublisherTest
    {
        [TestMethod]
        public void MessagePublisherCreatesChannel()
        {
            // Arrange
            Mock<IConnection> connectionMock = new Mock<IConnection>();
            Mock<IBusContext<IConnection>> contextMock = new Mock<IBusContext<IConnection>>();
            Mock<IModel> modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var publisher = new RabbitMqMessagePublisher(contextMock.Object);
            
            var message = new EventMessage();
            
            // Act
            publisher.SendMessage(message);
            
            // Assert
            connectionMock.Setup(e => e.CreateModel());
        }
        
        [TestMethod]
        public void MessagePublisherCreatesBasicProperties()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var publisher = new RabbitMqMessagePublisher(contextMock.Object);
            
            var message = new EventMessage();
            
            // Act
            publisher.SendMessage(message);
            
            // Assert
            modelMock.Verify(e => e.CreateBasicProperties());
        }
        
        [TestMethod]
        public void MessagePublisherCallsBasicPublish()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var publisher = new RabbitMqMessagePublisher(contextMock.Object);
            
            var message = new EventMessage();
            
            // Act
            publisher.SendMessage(message);
            
            // Assert
            modelMock.Verify(e => e.BasicPublish(It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<byte[]>()));
        }
        
        [TestMethod]
        [DataRow("test.exchange", "test.key", "SuperEvent", "Secret Data")]
        [DataRow("TestExchange", "TopicTest", "RandomEvent", "Not so secret data")]
        [DataRow("Blackjack.Exchange", "Blackjack.User.New", "SpelerToegevoegdEvent", "Jan Pomp is toegevoegd")]
        [DataRow("Blackjack.Exchange", "Blackjack.User.New", "DealerToegevoegdEvent", "Pieter is toegevoegd")]
        public void MessagePublisherCallsBasicPublishWithExpectedValues(string exchange, string routingKey, string type, string body)
        {
            // Arrange
            byte[] bodyBytes = Encoding.Unicode.GetBytes(body);
            
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            contextMock.SetupGet(e => e.ExchangeName).Returns(exchange);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var publisher = new RabbitMqMessagePublisher(contextMock.Object);
            
            Guid guid = Guid.NewGuid();
            
            var message = new EventMessage
            {
                Body = bodyBytes,
                EventType = type,
                CorrelationId = guid,
                Topic = routingKey
            };

            // Act
            publisher.SendMessage(message);
            
            // Assert
            modelMock.Verify(e => e.BasicPublish(exchange, 
                    routingKey, 
                    false, 
                    It.Is<IBasicProperties>(p => p.Type == type 
                                 && p.CorrelationId == guid.ToString() 
                                 && message.Timestamp.Equals(p.Timestamp.UnixTime)), 
                    bodyBytes)
            );
        }
    }
}