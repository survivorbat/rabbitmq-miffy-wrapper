using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.MicroServices.Host.HostEventArgs;
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
            var host = new MicroserviceHost(context, new MicroserviceListener[0], new MicroserviceCommandListener[0], "test.queue", logger.Object);

            // Assert
            Assert.AreSame(context, host.Context);
        }

        [TestMethod]
        public void DisposeCallsDisposeOnContext()
        {
            // Arrange
            const string queue = "test.queue";
            var contextMock = new Mock<IBusContext<IConnection>>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            var host = new MicroserviceHost(contextMock.Object, new MicroserviceListener[0], new MicroserviceCommandListener[0], queue, logger.Object);

            // Act
            host.Dispose();

            // Assert
            contextMock.Verify(e => e.Dispose(), Times.Once);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(4)]
        [DataRow(20)]
        public void CreateListenerIsCalledOnce(int listenerAmount)
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            contextMock.Setup(e => e.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(receiverMock.Object);

            var listeners = Enumerable.Range(0, listenerAmount).Select(e => new MicroserviceListener());

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), "test.queue", logger.Object);

            // Act
            host.Start();

            // Assert
            contextMock.Verify(e => e.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Once);
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

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topicNames} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queueName, logger.Object);

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

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            // Act
            host.Start();

            // Assert
            receiverMock.Verify(e => e.StartReceivingMessages());
        }

        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("some.queue.somewhere")]
        public void QueueIsProperlySet(string queueName)
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var logger = new Mock<ILogger<MicroserviceHost>>();
            var context = contextMock.Object;

            // Act
            var host = new MicroserviceHost(context, new MicroserviceListener[0], new MicroserviceCommandListener[0], queueName, logger.Object);

            // Assert
            Assert.AreSame(queueName, host.QueueName);
        }

        [TestMethod]
        public void PauseIsCalledOnReceiver()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            host.Start();

            // Act
            host.Pause();

            // Assert
            receiverMock.Verify(e => e.Pause());
        }

        [TestMethod]
        public void PausingHostSetsIsPausedToTrue()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            host.Start();

            // Act
            host.Pause();

            // Assert
            Assert.AreEqual(true, host.IsPaused);
        }

        [TestMethod]
        public void ResumingHostSetsIsPausedToFalse()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            host.Start();
            host.Pause();

            // Act
            host.Resume();

            // Assert
            Assert.AreEqual(false, host.IsPaused);
        }

        [TestMethod]
        public void IsPausedIsDefaultSetToFalse()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            // Act
            var host = new MicroserviceHost(contextMock.Object, new List<MicroserviceListener>(), new List<MicroserviceCommandListener>(), "testQueue", logger.Object);

            // Assert
            Assert.AreEqual(false, host.IsPaused);
        }

        [TestMethod]
        public void ResumeIsCalledOnReceiver()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            host.Start();
            host.Pause();

            // Act
            host.Resume();

            // Assert
            receiverMock.Verify(e => e.Resume());
        }

        [TestMethod]
        public void PausingWhileAlreadyPausedThrowsException()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            host.Start();
            host.Pause();

            // Act
            void Act() => host.Pause();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempted to pause the MicroserviceHost, but it was already paused.", exception.Message);
        }

        [TestMethod]
        public void PausingTriggersHostPausedEventWithProperValues()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            host.Start();

            IMicroserviceHost resultHost = null;
            HostPausedEventArgs eventArgs = null;
            host.HostPaused += (microserviceHost, args) =>
            {
                resultHost = microserviceHost;
                eventArgs = args;
            };

            // Act
            host.Pause();

            // Assert
            Assert.AreEqual(host, resultHost);
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void ResumingWhileNotPausedThrowsException()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);
            host.Start();

            // Act
            void Act() => host.Resume();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempted to resume the MicroserviceHost, but it wasn't paused.", exception.Message);
        }

        [TestMethod]
        public void PausingWhileNotStartedThrowsException()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            // Act
            void Act() => host.Pause();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempted to pause the MicroserviceHost, but host has not been started.", exception.Message);
        }

        [TestMethod]
        public void ResumingWhileNotStartedThrowsException()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            // Act
            void Act() => host.Resume();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempted to resume the MicroserviceHost, but host has not been started.", exception.Message);
        }

        [TestMethod]
        public void ResumingTriggersHostResumedEventWithProperValues()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            host.Start();
            host.Pause();

            IMicroserviceHost resultHost = null;
            HostResumedEventArgs eventArgs = null;
            host.HostResumed += (microserviceHost, args) =>
            {
                resultHost = microserviceHost;
                eventArgs = args;
            };

            // Act
            host.Resume();

            // Assert
            Assert.AreEqual(host, resultHost);
            Assert.IsNotNull(eventArgs);
        }

        [TestMethod]
        public void StartingTheHostASecondTimeThrowsException()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver("testQueue", topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);
            host.Start();

            // Act
            void Act() => host.Start();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempted to start the MicroserviceHost, but it has already started.", exception.Message);
        }

        [TestMethod]
        public void StartingTriggersHostStartedEventWithProperValues()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var receiverMock = new Mock<IMessageReceiver>();
            var logger = new Mock<ILogger<MicroserviceHost>>();

            const string queue = "testQueue";
            string[] topics = {"Topic1", "Topic2"};

            contextMock.Setup(e => e.CreateMessageReceiver(queue, topics))
                .Returns(receiverMock.Object);

            var listeners = new[] {new MicroserviceListener {TopicExpressions = topics} };

            var host = new MicroserviceHost(contextMock.Object, listeners, new List<MicroserviceCommandListener>(), queue, logger.Object);

            IMicroserviceHost resultHost = null;
            HostStartedEventArgs eventArgs = null;
            host.HostStarted += (microserviceHost, args) =>
            {
                resultHost = microserviceHost;
                eventArgs = args;
            };

            // Act
            host.Start();

            // Assert
            Assert.AreEqual(host, resultHost);
            Assert.IsNotNull(eventArgs);
        }
    }
}
