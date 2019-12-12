using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;
using Moq;

namespace Minor.Miffy.Test.Unit.TestBus
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
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);

            // Act
            var receiver = new TestCommandReceiver(context.Object, "queue");

            // Assert
            Assert.IsFalse(receiver.IsPaused);
        }

        [TestMethod]
        public void PausePausesReceiver()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var receiver = new TestCommandReceiver(context.Object, "queue");

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
            var receiver = new TestCommandReceiver(context.Object, "queue");

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
            var receiver = new TestCommandReceiver(context.Object, "queue");

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
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var receiver = new TestCommandReceiver(context.Object, "queue");

            receiver.Pause();

            // Act
            void Act() => receiver.Pause();

            // Assert
            BusConfigurationException exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Attempting to pause the TestCommandReceiver, but it was already paused.", exception.Message);
        }

    }
}
