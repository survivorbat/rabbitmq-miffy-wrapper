using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Host;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Test.Unit.Host
{
    [TestClass]
    public class MicroserviceHostTest
    {
        [TestMethod]
        public void ContextIsProperlySet()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var logger = new Mock<ILogger<MicroserviceHost>>();
            var context = contextMock.Object;

            // Act
            var host = new MicroserviceHost(context, null, null, logger.Object);

            // Assert
            Assert.AreSame(context, host.Context);
        }

        [TestMethod]
        public void DisposeCallsDisposeOnContext()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var logger = new Mock<ILogger<MicroserviceHost>>();
            var context = contextMock.Object;
            var host = new MicroserviceHost(context, null, null, logger.Object);

            // Act
            host.Dispose();

            // Assert
            contextMock.Verify(e => e.Dispose(), Times.Once);
        }

        [TestMethod]
        public void DisposeIsCalledOnAllEventListeners()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var logger = new Mock<ILogger<MicroserviceHost>>();
            var context = contextMock.Object;
            var host = new MicroserviceHost(context, null, null, logger.Object);

            // Act
            host.Dispose();

            // Assert
            contextMock.Verify(e => e.Dispose(), Times.Once);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(4)]
        [DataRow(20)]
        public void CreateListenerIsCalledForAllGivenListeners(int listenerAmount)
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            contextMock.Setup(e => e.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(receiverMock.Object);

            var listeners = Enumerable.Range(0, listenerAmount).Select(e => new MicroserviceListener());

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), logger.Object);

            // Act
            host.Start();

            // Assert
            contextMock.Verify(e => e.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Exactly(listenerAmount));
        }

        [TestMethod]
        [DataRow("foo", "bar,bez")]
        [DataRow("bar", "foo,foo#")]
        public void CreateListenerIsCalledWithProperParameters(string queueName, string topics)
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            string[] topicNames = topics.Split(',');

            contextMock.Setup(e => e.CreateMessageReceiver(queueName, topicNames))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {Queue = queueName, TopicExpressions = topicNames} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), logger.Object);

            // Act
            host.Start();

            // Assert
            contextMock.Verify(e => e.CreateMessageReceiver(queueName, topicNames));
        }

        [TestMethod]
        public void StartListeningIsCalledOnReceiver()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver("testQueue", topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {Queue = queue, TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), logger.Object);

            // Act
            host.Start();

            // Assert
            receiverMock.Verify(e => e.StartReceivingMessages());
        }
    }
}
