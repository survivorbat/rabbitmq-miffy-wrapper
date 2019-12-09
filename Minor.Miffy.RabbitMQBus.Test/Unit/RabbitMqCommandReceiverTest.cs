using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

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
            _ = new RabbitMqCommandReceiver(contextMock.Object, "test.queue");

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

        [TestMethod]
        [DataRow("reply.queue", "TestType", "Hello World")]
        [DataRow("queue.reply", "DummyType", "Goodbye World")]
        [DataRow("QueueToReplyTo", "CoolType", "Bonjour World")]
        public void StartReceivingCommandsConsumerCallsCallback(string replyQueue, string type, string body)
        {
            // Arrange
            byte[] byteBody = Encoding.Unicode.GetBytes(body);

            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var receiver = new RabbitMqCommandReceiver(contextMock.Object, "test.queue");

            CommandMessage result = new CommandMessage();

            IBasicConsumer consumer = null;

            // Retrieve consumer from callback
            modelMock.Setup(e => e.BasicConsume("test.queue", false, "", false, false, null, It.IsAny<IBasicConsumer>()))
                .Callback<string,bool,string,bool,bool, IDictionary<string,object>, IBasicConsumer>((a, b, c, d, e, f, givenConsumer) => consumer = givenConsumer);

            Guid guid = Guid.NewGuid();
            var properties = new BasicProperties
            {
                ReplyTo = replyQueue,
                Type = type,
                CorrelationId = guid.ToString()
            };

            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands(e => result = e);

            // Act
            consumer.HandleBasicDeliver("", 0, false, "test.exchange", "test.queue", properties, byteBody);

            // Assert
            Assert.AreEqual(byteBody, result.Body);
            Assert.AreEqual(guid, result.CorrelationId);
            Assert.AreEqual(type, result.EventType);
            Assert.AreEqual(replyQueue, result.ReplyQueue);
        }

        [TestMethod]
        [DataRow("Random exception was thrown")]
        [DataRow("Apocalyptic exception was thrown")]
        [DataRow("Ancient Exception")]
        public void StartReceivingCommandsConsumerCatchesExceptionWithCommandErrorAndMessage(string message)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var receiver = new RabbitMqCommandReceiver(contextMock.Object, "test.queue");

            IBasicConsumer consumer = null;

            // Retrieve consumer from callback
            modelMock.Setup(e => e.BasicConsume("test.queue", false, "", false, false, null, It.IsAny<IBasicConsumer>()))
                .Callback<string,bool,string,bool,bool, IDictionary<string,object>, IBasicConsumer>((a, b, c, d, e, f, givenConsumer) => consumer = givenConsumer);

            // Retrieve basic publish data
            byte[] resultBody = null;
            modelMock.Setup(e => e.BasicPublish(It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                It.IsAny<IBasicProperties>(),
                It.IsAny<byte[]>()))
                .Callback<string, string, bool, IBasicProperties, byte[]>((a, b, c, d, body) => resultBody = body);

            var properties = new BasicProperties { CorrelationId = Guid.NewGuid().ToString() };

            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands(e => throw new Exception(message));

            // Act
            consumer.HandleBasicDeliver("", 0, false, "test.exchange", "test.queue", properties, new byte[0]);

            // Assert
            var stringBody = Encoding.Unicode.GetString(resultBody);
            var commandError = JsonConvert.DeserializeObject<CommandError>(stringBody);
            Assert.AreEqual(message, commandError.Exception.Message);
        }

        [TestMethod]
        [DataRow("reply.queue", "Hello World")]
        [DataRow("QueueToReply", "GoodBye World")]
        public void ConsumerCallsBasicPublishWithValues(string replyTo, string message)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var receiver = new RabbitMqCommandReceiver(contextMock.Object, "test.queue");

            IBasicConsumer consumer = null;

            // Retrieve consumer from callback
            modelMock.Setup(e => e.BasicConsume("test.queue", false, "", false, false, null, It.IsAny<IBasicConsumer>()))
                .Callback<string,bool,string,bool,bool, IDictionary<string,object>, IBasicConsumer>((a, b, c, d, e, f, givenConsumer) => consumer = givenConsumer);

            var properties = new BasicProperties
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ReplyTo = replyTo
            };

            var responseMessage = new CommandMessage {Body = Encoding.Unicode.GetBytes(message) };

            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands(e => responseMessage);

            // Act
            consumer.HandleBasicDeliver("", 0, false, "", "test.queue", properties, new byte[0]);

            // Assert
            var jsonResponse = JsonConvert.SerializeObject(responseMessage);
            var bodyResponse = Encoding.Unicode.GetBytes(jsonResponse);
            modelMock.Verify(e => e.BasicPublish("",
                replyTo,
                false,
                It.IsAny<IBasicProperties>(),
                bodyResponse));
        }

        [TestMethod]
        [DataRow("OopsieException")]
        [DataRow("ExplosionException")]
        [DataRow("DeathException")]
        public void ExceptionIsProperlyHandled(string exceptionMessage)
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            contextMock.SetupGet(e => e.ExchangeName).Returns("test.exchange");
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var receiver = new RabbitMqCommandReceiver(contextMock.Object, "test.queue");

            IBasicConsumer consumer = null;

            // Retrieve consumer from callback
            modelMock.Setup(e => e.BasicConsume("test.queue", false, "", false, false, null, It.IsAny<IBasicConsumer>()))
                .Callback<string,bool,string,bool,bool, IDictionary<string,object>, IBasicConsumer>((a, b, c, d, e, f, givenConsumer) => consumer = givenConsumer);

            var properties = new BasicProperties
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ReplyTo = "reply.queue"
            };

            var exception = new Exception(exceptionMessage);
            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands(e => throw exception);

            var expectedBody = new byte[0];
            modelMock.Setup(e => e.BasicPublish("",
                "reply.queue",
                false,
                It.IsAny<IBasicProperties>(),
                It.IsAny<byte[]>()))
                .Callback<string, string, bool, IBasicProperties, byte[]>((a, b, c, d, body) => expectedBody = body);

            // Act
            consumer.HandleBasicDeliver("", 0, false, "", "test.queue", properties, new byte[0]);

            // Assert
            var stringBody = Encoding.Unicode.GetString(expectedBody);
            CommandError error = JsonConvert.DeserializeObject<CommandError>(stringBody);

            Assert.AreEqual(error.Exception?.Message, exceptionMessage);
        }

        [TestMethod]
        [DataRow(593L)]
        [DataRow(5352593L)]
        [DataRow(535252467893L)]
        public void ConsumerCallsBasicAckWithDeliveryTag(long deliveryTagLong)
        {
            // Arrange
            ulong deliveryTag = UInt64.Parse(deliveryTagLong.ToString());

            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();
            var modelMock = new Mock<IModel>();

            contextMock.SetupGet(e => e.Connection).Returns(connectionMock.Object);
            connectionMock.Setup(e => e.CreateModel()).Returns(modelMock.Object);
            modelMock.Setup(e => e.CreateBasicProperties()).Returns(new BasicProperties());

            var receiver = new RabbitMqCommandReceiver(contextMock.Object, "test.queue");

            IBasicConsumer consumer = null;

            // Retrieve consumer from callback
            modelMock.Setup(e => e.BasicConsume("test.queue", false, "", false, false, null, It.IsAny<IBasicConsumer>()))
                .Callback<string,bool,string,bool,bool, IDictionary<string,object>, IBasicConsumer>((a, b, c, d, e, f, givenConsumer) => consumer = givenConsumer);

            var properties = new BasicProperties { CorrelationId = Guid.NewGuid().ToString() };

            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands(e => new CommandMessage());

            // Act
            consumer.HandleBasicDeliver("", deliveryTag, false, "test.exchange", "test.queue", properties, new byte[0]);

            // Assert
            modelMock.Verify(e => e.BasicAck(deliveryTag, false));
        }
    }
}
