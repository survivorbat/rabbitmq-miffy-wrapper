using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;

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
    }
}
