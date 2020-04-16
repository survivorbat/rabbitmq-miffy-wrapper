using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqCommandPublisherTest
    {
        private const int WaitTime = 1500;

        [TestMethod]
        public void SendCommandCreatesChannel()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);

            var sender = new RabbitMqCommandSender(contextMock.Object);

            var command = new CommandMessage();

            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(WaitTime);

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

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);

            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            var sender = new RabbitMqCommandSender(contextMock.Object);

            var command = new CommandMessage();

            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(WaitTime);

            // Assert
            modelMock.Verify(e =>
                e.QueueDeclare("", true, false, true, null), Times.Once());
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

            contextMock.SetupGet(e => e.ExchangeName).Returns(exchangeName);

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);

            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));

            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var sender = new RabbitMqCommandSender(contextMock.Object);

            var command = new CommandMessage();

            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(WaitTime);

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
        [DataRow("TestQueue", "ExchangeTest")]
        public void SendCommandCallsBasicPublishThrowsExceptionOnTimeout(string queueName, string exchangeName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.ExchangeName).Returns(exchangeName);

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);

            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));

            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var sender = new RabbitMqCommandSender(contextMock.Object);

            var command = new CommandMessage
            {
                DestinationQueue = queueName
            };

            // Act
            async Task<CommandMessage> Act() => await sender.SendCommandAsync(command);

            // Assert
            var exception = Assert.ThrowsExceptionAsync<MessageTimeoutException>(Act);
            Assert.AreEqual($"No response received from queue {queueName} after {RabbitMqCommandSender.CommandTimeout}ms", exception.Result.Message);
        }

        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("queue.test")]
        public void SendCommandCallsBasicPublish(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);

            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));

            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var sender = new RabbitMqCommandSender(contextMock.Object);

            var command = new CommandMessage
            {
                DestinationQueue = queueName
            };

            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(WaitTime);

            // Assert
            modelMock.Verify(e => e.BasicPublish("", queueName, true, It.IsAny<BasicProperties>(), It.IsAny<byte[]>()));
        }

        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("queue.test")]
        public void ConsumerExitsIfCorrelationIdIsNotCorrect(string queueName)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.ExchangeName).Returns(queueName);
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);

            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));

            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            IBasicConsumer consumer = null;

            modelMock.Setup(e => e.BasicConsume(queueName, false, It.IsAny<string>(), false,
                false, null, It.IsAny<EventingBasicConsumer>()))
                .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((a, b, c, d, e, f, receivedConsumer) => consumer = receivedConsumer);

            var sender = new RabbitMqCommandSender(contextMock.Object);

            var command = new CommandMessage
            {
                DestinationQueue = queueName
            };

            sender.SendCommandAsync(command);

            Thread.Sleep(WaitTime);

            BasicProperties testBasicProperties = new BasicProperties {CorrelationId = "ID"};

            // Act
            consumer.HandleBasicDeliver("tag",
                20,
                false,
                "",
                "",
                testBasicProperties,
                new byte[0]);

            // Assert
            modelMock.Verify(e => e.BasicAck(20, false), Times.Never);
        }

        [TestMethod]
        [DataRow("NullReferenceException")]
        [DataRow("Something terrible happened!")]
        public void ExceptionIsThrownIfEventTypeIsCommandError(string exceptionMessage)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.ExchangeName).Returns("test.queue");
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk("test.queue", 0, 0));

            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            IBasicConsumer consumer = null;

            modelMock.Setup(e => e.BasicConsume("test.queue", false, It.IsAny<string>(), false,
                false, null, It.IsAny<EventingBasicConsumer>()))
                .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((a, b, c, d, e, f, receivedConsumer) => consumer = receivedConsumer);

            var sender = new RabbitMqCommandSender(contextMock.Object);

            Guid guid = Guid.NewGuid();
            var command = new CommandMessage
            {
                DestinationQueue = "test.queue",
                CorrelationId = guid,
                ReplyQueue = "reply.queue"
            };

            BasicProperties testBasicProperties = new BasicProperties {CorrelationId = guid.ToString()};

            var body = new CommandError
            {
                EventType = "CommandError",
                Exception = new Exception(exceptionMessage),
                CorrelationId = guid
            };

            var bodyJson = JsonConvert.SerializeObject(body);

            // Ensure that response arrives 3 seconds later
            Task.Run(() =>
            {
                Thread.Sleep(3000);
                consumer.HandleBasicDeliver("tag",
                    20,
                    false,
                    "",
                    "",
                    testBasicProperties,
                    Encoding.Unicode.GetBytes(bodyJson));
            });

            // Act
            Task Act() => sender.SendCommandAsync(command);

            // Assert
            Task<DestinationQueueException> exception = Assert.ThrowsExceptionAsync<DestinationQueueException>(Act);
            Assert.AreEqual("Received error command from queue test.queue", exception.Result.Message);
        }

        [TestMethod]
        [DataRow("a9daf3b7-03f7-4593-877d-e9b541f2a0e1")]
        [DataRow("9c304340-3903-4cf3-96d1-2a8d333ee789")]
        [DataRow("2377db9d-5e3c-4a8b-9f98-0c567aa812f8")]
        [DataRow("d415f231-a3db-4b26-9c41-ab82638a249a")]
        public void CommandMessageIsReturnedProperly(string correlationId)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.ExchangeName).Returns("test.queue");
            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.QueueDeclare("", true, false, true, null))
                .Returns(new QueueDeclareOk("test.queue", 0, 0));

            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            IBasicConsumer consumer = null;

            modelMock.Setup(e => e.BasicConsume("test.queue", false, It.IsAny<string>(), false,
                false, null, It.IsAny<EventingBasicConsumer>()))
                .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((a, b, c, d, e, f, receivedConsumer) => consumer = receivedConsumer);

            var sender = new RabbitMqCommandSender(contextMock.Object);

            var command = new CommandMessage
            {
                DestinationQueue = "test.queue",
                CorrelationId = Guid.Parse(correlationId)
            };

            var result = sender.SendCommandAsync(command);

            Thread.Sleep(WaitTime);

            BasicProperties testBasicProperties = new BasicProperties {CorrelationId = correlationId};

            var body = new CommandError
            {
                EventType = "DummyCommand",
                Exception = new Exception("TestException"),
                CorrelationId = Guid.Parse(correlationId)
            };

            var bodyJson = JsonConvert.SerializeObject(body);

            // Act
            consumer.HandleBasicDeliver("tag",
                20,
                false,
                "",
                "",
                testBasicProperties,
                Encoding.Unicode.GetBytes(bodyJson));

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(CommandMessage));
        }
    }
}
