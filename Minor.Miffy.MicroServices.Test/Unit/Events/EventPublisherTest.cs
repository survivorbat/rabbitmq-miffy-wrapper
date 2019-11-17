using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Events;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Test.Unit.Events
{
    [TestClass]
    public class EventPublisherTest
    {
        [TestMethod]
        public void PublishCallsSendMessageOnSender()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var senderMock = new Mock<IMessageSender>();

            contextMock.Setup(e => e.CreateMessageSender())
                .Returns(senderMock.Object);
            
            var publisher = new EventPublisher(contextMock.Object);
            
            var domainEvent = new TestEvent("Topic");
            
            // Act
            publisher.Publish(domainEvent);
            
            // Assert
            senderMock.Verify(e => e.SendMessage(It.IsAny<EventMessage>()), Times.Once);
        }
        
        [TestMethod]
        [DataRow("School", "Test data")]
        [DataRow("MVM.Blackjack", "Nijntje, lief klein konijntje")]
        [DataRow("MVM.Blackjack", "Nieuwe speler toegevoegd")]
        [DataRow("EDA.Start.Machine", "The machine has started!")]
        public void PublishCallsSendMessageWithExpectedMessageOnSender(string topic, string body)
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var senderMock = new Mock<IMessageSender>();

            contextMock.Setup(e => e.CreateMessageSender())
                .Returns(senderMock.Object);

            EventMessage result = new EventMessage();
            
            senderMock.Setup(e => e.SendMessage(It.IsAny<EventMessage>()))
                .Callback<EventMessage>(e => result = e);
            
            var publisher = new EventPublisher(contextMock.Object);
            
            var domainEvent = new TestEvent(topic) { DataField = body };
            var jsonBody = JsonConvert.SerializeObject(domainEvent);
            
            // Act
            publisher.Publish(domainEvent);
            
            // Assert
            Assert.AreEqual(topic, result.Topic);
            Assert.AreEqual(jsonBody, Encoding.Unicode.GetString(result.Body));
            Assert.AreEqual(domainEvent.Timestamp, result.Timestamp);
            Assert.AreEqual(domainEvent.Id, result.CorrelationId);
        }
    }
}