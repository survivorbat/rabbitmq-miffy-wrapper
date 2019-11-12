using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.Test.Unit.TestBus
{
    [TestClass]
    public class TestBusContextTest
    {
        [TestMethod]
        public void CreateMessageSenderReturnsMessageSender()
        {
            // Arrange
            var context = new TestBusContext();
            
            // Act
            var result = context.CreateMessageSender();
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(TestMessageSender));
        }

        [TestMethod]
        public void CreateMessageReceiverReturnsMessageReceiver()
        {
            // Arrange
            var context = new TestBusContext();
            
            // Act
            var result = context.CreateMessageReceiver("test.queue", new string[0]);
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(TestMessageReceiver));
        }
    }
}