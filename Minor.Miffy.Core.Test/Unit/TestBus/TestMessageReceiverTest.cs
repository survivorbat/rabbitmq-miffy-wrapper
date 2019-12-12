using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;
using Moq;

namespace Minor.Miffy.Test.Unit.TestBus
{
    [TestClass]
    public class TestMessageReceiverTest
    {
        [TestMethod]
        [DataRow("test", "test")]
        [DataRow("foo", "test,test,test")]
        [DataRow("bar", "foo,bar,zed")]
        public void ConstructorParametersAreProperlySet(string queue, string topics)
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>();
            string[] topicNames = topics.Split(',');

            // Act
            var receiver = new TestMessageReceiver(context.Object, queue, topicNames);

            // Assert
            Assert.AreSame(context.Object, receiver.Context);
            Assert.AreSame(queue, receiver.QueueName);
            Assert.AreSame(topicNames,  receiver.TopicFilters);
        }

        [TestMethod]
        public void StartListeningShouldThrowExceptionWhenAlreadyListening()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>();
            string[] topics = new string[0];
            string queue = "queue.name";

            var receiver = new TestMessageReceiver(context.Object, queue, topics);
            receiver.StartReceivingMessages();

            // Act
            void Act() => receiver.StartReceivingMessages();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Receiver is already listening to events!", exception.Message);
        }

        [TestMethod]
        [DataRow("test", "test")]
        [DataRow("foo", "test,test,test")]
        [DataRow("bar", "foo,bar,zed")]
        [DataRow("zed", "foo*")]
        [DataRow("lorem", "foo#.bar,bar*")]
        [DataRow("ipsum", "foo#..***..")]
        public void StartReceivingCreatesQueueWrapperInDictionary(string queue, string topics)
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var dictionary = new Dictionary<TestBusKey, TestBusQueueWrapper<EventMessage>>();

            context.SetupGet(e => e.DataQueues)
                .Returns(dictionary);

            string[] topicNames = topics.Split(',');

            var receiver = new TestMessageReceiver(context.Object, queue, topicNames);

            // Act
            receiver.StartReceivingMessages();

            // Assert
            foreach (var topic in topicNames)
            {
                var key = new TestBusKey(queue, topic);
                Assert.IsTrue(dictionary.ContainsKey(key));
            }
        }

        [TestMethod]
        public void ReceiverIsStandardNotPaused()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);

            // Act
            var receiver = new TestMessageReceiver(context.Object, "queue", new string[0]);

            // Assert
            Assert.IsFalse(receiver.IsPaused);
        }

        [TestMethod]
        public void PausePausesReceiver()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var receiver = new TestMessageReceiver(context.Object, "queue", new string[0]);
            receiver.StartReceivingMessages();

            // Act
            receiver.Pause();

            // Assert
            Assert.IsTrue(receiver.IsPaused);
        }

        [TestMethod]
        public void ResumeResumesReceiver()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var receiver = new TestMessageReceiver(context.Object, "queue", new string[0]);
            receiver.StartReceivingMessages();

            receiver.Pause();

            // Act
            receiver.Resume();

            // Assert
            Assert.IsFalse(receiver.IsPaused);
        }

        [TestMethod]
        public void ResumeThrowsExceptionIfNotPaused()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var receiver = new TestMessageReceiver(context.Object, "queue", new string[0]);
            receiver.StartReceivingMessages();

            // Act
            void Act() => receiver.Resume();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to resume the TestMessageReceiver, but it was not paused.", exception.Message);
        }

        [TestMethod]
        public void PauseThrowsExceptionIfAlreadyPaused()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var receiver = new TestMessageReceiver(context.Object, "queue", new string[0]);
            receiver.StartReceivingMessages();

            receiver.Pause();

            // Act
            void Act() => receiver.Pause();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to pause the TestMessageReceiver, but it was already paused.", exception.Message);
        }

        [TestMethod]
        public void PauseThrowsExceptionWhenNotListening()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var receiver = new TestMessageReceiver(context.Object, "queue", new string[0]);

            // Act
            void Act() => receiver.Pause();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to pause the TestMessageReceiver, but it is not even receiving messages.", exception.Message);
        }

        [TestMethod]
        public void ResumeThrowsExceptionWhenNotListening()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var receiver = new TestMessageReceiver(context.Object, "queue", new string[0]);

            // Act
            void Act() => receiver.Resume();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to resume the TestMessageReceiver, but it is not even receiving messages.", exception.Message);
        }
    }
}
