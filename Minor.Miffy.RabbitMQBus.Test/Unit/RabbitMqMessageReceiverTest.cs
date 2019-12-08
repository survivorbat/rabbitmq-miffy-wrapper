using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqMessageReceiverTest
    {
        private const int WaitTime = 1500;

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
            var result = new RabbitMqMessageReceiver(contextMock.Object, queueName, null);

            // Assert
            Assert.AreEqual(queueName, result.QueueName);
        }

        [TestMethod]
        [DataRow("test,topic,random")]
        [DataRow("routing,key,is,what,we,call,it")]
        public void TopicExpressionsIsProperlySet(string topics)
        {
            // Arrange
            var topicNames = topics.Split(',');
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);

            // Act
            var result = new RabbitMqMessageReceiver(contextMock.Object, "test.queue", topicNames);

            // Assert
            Assert.AreSame(topicNames, result.TopicFilters);
        }

        [TestMethod]
        public void CreateModelIsCalledInConstructor()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);

            // Act
            _ = new RabbitMqMessageReceiver(contextMock.Object, "test.queue", null);

            // Assert
            connectionMock.Verify(e => e.CreateModel());
        }

        [TestMethod]
        public void StartReceivingMessagesCanOnlyBeCalledOnce()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var receiver = new RabbitMqMessageReceiver(contextMock.Object, "test.queue", new string[0]);

            receiver.StartReceivingMessages();

            // Act
            void Act() => receiver.StartReceivingMessages();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Receiver is already listening to events!", exception.Message);
        }

        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("queue.test")]
        public void StartReceivingMessagesDeclaresQueue(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var receiver = new RabbitMqMessageReceiver(contextMock.Object, queueName, new string[0]);

            // Act
            receiver.StartReceivingMessages();

            // Assert
            modelMock.Verify(e => e.QueueDeclare(queueName, true, false, false, null));
        }

        [TestMethod]
        [DataRow("test.exchange", "test.queue", "huis,boom,beest")]
        [DataRow("TestExchange", "QueueTest", "blackjack,player")]
        [DataRow("ExchangeEverything", "QueueEverything", "#,#,#")]
        public void StartReceivingMessagesBindsQueueToTopics(string exchangeName, string queueName, string topics)
        {
            // Arrange
            var topicNames = topics.Split(',');
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            contextMock.Setup(e => e.ExchangeName).Returns(exchangeName);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var receiver = new RabbitMqMessageReceiver(contextMock.Object, queueName, topicNames);

            // Act
            receiver.StartReceivingMessages();

            // Assert
            foreach (string topic in topicNames)
            {
                modelMock.Verify(e => e.QueueBind(queueName, exchangeName, topic, null));
            }
        }

        [TestMethod]
        public void StartHandlingMessagesThrowsExceptionOnNotListening()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var receiver = new RabbitMqMessageReceiver(contextMock.Object, "test.queue", null);

            // Act
            void Act() => receiver.StartHandlingMessages(e => {});

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Receiver is not listening to events", exception.Message);
        }

        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("TestQueue")]
        [DataRow("queue.that.is.cool")]
        public void StartHandlingMessagesCallsBasicConsume(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var receiver = new RabbitMqMessageReceiver(contextMock.Object, queueName, new string[0]);

            receiver.StartReceivingMessages();

            // Act
            receiver.StartHandlingMessages(e => {});

            // Assert
            modelMock.Verify(e => e.BasicConsume(queueName, true, "", false, false, null, It.IsAny<IBasicConsumer>()));
        }

        [TestMethod]
        [DataRow("TestExchange", "TestTopic", "SuperEvent")]
        [DataRow("RandomExchange", "RandomTopic", "TestEvent")]
        public void CallbackIsCalledWithProperMessage(string exchangeName, string topic, string eventType)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            IBasicConsumer consumer = null;

            // Retrieve consumer from callback
            var receiver = new RabbitMqMessageReceiver(contextMock.Object, "test.queue", new string[0]);
            modelMock.Setup(e => e.BasicConsume("test.queue", true, "", false, false, null, It.IsAny<IBasicConsumer>()))
                .Callback<string,bool,string,bool,bool, IDictionary<string,object>, IBasicConsumer>((a, b, c, d, e, f, givenConsumer) => consumer = givenConsumer);

            EventMessage eventMessage = null;

            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => eventMessage = e);

            Guid guid = Guid.NewGuid();
            IBasicProperties properties = new BasicProperties
            {
                CorrelationId = guid.ToString(),
                Type = eventType
            };

            // Act
            Thread.Sleep(WaitTime);
            consumer.HandleBasicDeliver("", 0, false, exchangeName, topic, properties, new byte[0]);

            // Assert
            Assert.AreEqual(topic, eventMessage.Topic);
            Assert.AreEqual(eventType, eventMessage.EventType);
            Assert.AreEqual(guid, eventMessage.CorrelationId);
        }

        [TestMethod]
        public void DisposeIsCalledOnModel()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var receiver = new RabbitMqMessageReceiver(contextMock.Object, "test.queue", new string[0]);

            // Act
            receiver.Dispose();

            // Assert
            modelMock.Verify(e => e.Dispose());
        }
    }
}
