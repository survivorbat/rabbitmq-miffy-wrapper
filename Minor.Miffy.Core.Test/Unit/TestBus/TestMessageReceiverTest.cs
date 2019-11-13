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
        public void ConstructorParametersAreProperlySet()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>();
            IEnumerable<string> topics = new List<string> {"Bob", "Jan", "Piet"};
            string queue = "queue.name";
            
            // Act
            var receiver = new TestMessageReceiver(context.Object, queue, topics);
            
            // Assert
            Assert.AreSame(context.Object, receiver.Context);
            Assert.AreSame(queue, receiver.QueueName);
            Assert.AreSame(topics,  receiver.TopicFilters);
        }

        [TestMethod]
        public void StartListeningShouldThrowExceptionWhenAlreadyListening()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>();
            IEnumerable<string> topics = new List<string> {"Bob", "Jan", "Piet"};
            string queue = "queue.name";
            
            var receiver = new TestMessageReceiver(context.Object, queue, topics);
            receiver.StartReceivingMessages();
            
            // Act
            void Act() => receiver.StartReceivingMessages();
            
            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Receiver is already listening to events!", exception.Message);
        }
    }
}