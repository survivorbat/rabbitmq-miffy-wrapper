using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqMessageReceiverTest
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
            new RabbitMqMessageReceiver(contextMock.Object, "test.queue", null);

            // Assert
            connectionMock.Setup(e => e.CreateModel());
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
            modelMock.Verify(e => e.QueueDeclare(queueName, true, false, true, null));
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
                modelMock.Setup(e => e.QueueBind(queueName, exchangeName, topic, null));
            }
        }
    }
}