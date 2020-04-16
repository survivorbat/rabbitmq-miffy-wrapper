using Microsoft.VisualStudio.TestTools.UnitTesting;
using Miffy.TestBus;
using Moq;

namespace Miffy.Test.Unit.TestBus
{
    [TestClass]
    public class TestCommandReceiverTest
    {
        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("TestQueue")]
        public void ExceptionIsThrownOnCommandQueueNotDeclared(string queueName)
        {
            // Arrange
            var testContext = new TestBusContext();
            var testCommandreceiver = new TestCommandReceiver(testContext, queueName);

            // Act
            void Act() => testCommandreceiver.StartReceivingCommands(e => e);

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual($"Queue {queueName} has not been declared yet.", exception.Message);
        }

        [TestMethod]
        public void ReceiverIsStandardNotPaused()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>();

            // Act
            var receiver = new TestCommandReceiver(context.Object, "queue");

            // Assert
            Assert.IsFalse(receiver.IsPaused);
        }

        [TestMethod]
        public void PausePausesReceiver()
        {
            // Arrange
            var context = new TestBusContext();
            var receiver = new TestCommandReceiver(context, "queue");
            receiver.DeclareCommandQueue();

            // Act
            receiver.Pause();

            // Assert
            Assert.IsTrue(receiver.IsPaused);
        }

        [TestMethod]
        public void ResumeResumesReceiver()
        {
            // Arrange
            var context = new TestBusContext();
            var receiver = new TestCommandReceiver(context, "queue");
            receiver.DeclareCommandQueue();

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
            var context = new TestBusContext();
            var receiver = new TestCommandReceiver(context, "queue");
            receiver.DeclareCommandQueue();

            // Act
            void Act() => receiver.Resume();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to resume the TestCommandReceiver, but it was not paused.", exception.Message);
        }

        [TestMethod]
        public void PauseThrowsExceptionIfAlreadyPaused()
        {
            // Arrange
            var context = new TestBusContext();
            var receiver = new TestCommandReceiver(context, "queue");
            receiver.DeclareCommandQueue();

            receiver.Pause();

            // Act
            void Act() => receiver.Pause();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to pause the TestCommandReceiver, but it was already paused.", exception.Message);
        }

        [TestMethod]
        public void PauseThrowsExceptionWhenQueueNotDeclared()
        {
            // Arrange
            var context = new TestBusContext();
            var receiver = new TestCommandReceiver(context, "queue");

            // Act
            void Act() => receiver.Pause();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to pause the TestCommandReceiver, but it is not even receiving messages.", exception.Message);
        }

        [TestMethod]
        public void ResumeThrowsExceptionWhenQueueNotDeclared()
        {
            // Arrange
            var context = new TestBusContext();
            var receiver = new TestCommandReceiver(context, "queue");

            // Act
            void Act() => receiver.Resume();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to resume the TestCommandReceiver, but it is not even receiving messages.", exception.Message);
        }
    }
}
