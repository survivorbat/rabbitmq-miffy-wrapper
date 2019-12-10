using System;
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
            Mock<IBusContext<IConnection>> contextMock = new Mock<IBusContext<IConnection>>();
            Mock<IMessageSender> senderMock = new Mock<IMessageSender>();

            contextMock.Setup(e => e.CreateMessageSender())
                .Returns(senderMock.Object);

            EventPublisher publisher = new EventPublisher(contextMock.Object);

            TestEvent domainEvent = new TestEvent("Topic");

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
            Mock<IBusContext<IConnection>> contextMock = new Mock<IBusContext<IConnection>>();
            Mock<IMessageSender> senderMock = new Mock<IMessageSender>();

            contextMock.Setup(e => e.CreateMessageSender())
                .Returns(senderMock.Object);

            EventMessage result = new EventMessage();

            senderMock.Setup(e => e.SendMessage(It.IsAny<EventMessage>()))
                .Callback<EventMessage>(e => result = e);

            EventPublisher publisher = new EventPublisher(contextMock.Object);

            TestEvent domainEvent = new TestEvent(topic) { DataField = body };
            string jsonBody = JsonConvert.SerializeObject(domainEvent);

            // Act
            publisher.Publish(domainEvent);

            // Assert
            Assert.AreEqual(topic, result.Topic);
            Assert.AreEqual(jsonBody, Encoding.Unicode.GetString(result.Body));
            Assert.AreEqual(domainEvent.Timestamp, result.Timestamp);
            Assert.AreEqual(domainEvent.Id, result.CorrelationId);
        }

        [TestMethod]
        [DataRow(1000, "School.Test", "SchoolTest", "Test data")]
        [DataRow(2000, "MVM.Blackjack", "BlackJack", "Nijntje, lief klein konijntje")]
        [DataRow(32532525, "MVM.Blackjack", "BlackJackObject", "Nieuwe speler toegevoegd")]
        public void PublishWithRawDataCallsSendMessageWithExpectedMessage(long timestamp, string topic, string eventType, string body)
        {
            // Arrange
            Mock<IBusContext<IConnection>> contextMock = new Mock<IBusContext<IConnection>>();
            Mock<IMessageSender> senderMock = new Mock<IMessageSender>();

            contextMock.Setup(e => e.CreateMessageSender())
                .Returns(senderMock.Object);

            EventMessage result = new EventMessage();
            senderMock.Setup(e => e.SendMessage(It.IsAny<EventMessage>()))
                .Callback<EventMessage>(e => result = e);

            EventPublisher publisher = new EventPublisher(contextMock.Object);

            Guid calculatedGuid = Guid.NewGuid();

            // Act
            publisher.Publish(timestamp, topic, calculatedGuid, eventType, body);

            // Assert
            Assert.AreEqual(timestamp, result.Timestamp);
            Assert.AreEqual(topic, result.Topic);
            Assert.AreEqual(eventType, result.EventType);
            Assert.AreEqual(body, Encoding.Unicode.GetString(result.Body));
            Assert.AreEqual(calculatedGuid, result.CorrelationId);
        }
    }
}
