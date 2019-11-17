using System;
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
        
        [TestMethod]
        public void CreateMessageSenderReturnsCommandSender()
        {
            // Arrange
            var context = new TestBusContext();
            
            // Act
            var result = context.CreateCommandSender();
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(TestCommandSender));
        }

        [TestMethod]
        public void CreateCommandReceiverReturnsCommandReceiver()
        {
            // Arrange
            var context = new TestBusContext();
            
            // Act
            var result = context.CreateCommandReceiver("test.queue");
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(TestCommandReceiver));
        }
        
        [TestMethod]
        [DataRow("test", "test")]
        [DataRow("foo", "test,test,test")]
        [DataRow("bar", "foo,bar,zed")]
        public void CreateMessageReceiverReturnsInitializedReceiver(string queueName, string topics)
        {
            // Arrange
            var context = new TestBusContext();

            string[] topicList = topics.Split(',');

            // Act
            IMessageReceiver receiver = context.CreateMessageReceiver(queueName, topicList);

            // Assert
            Assert.AreSame(queueName, receiver.QueueName);
            Assert.AreSame(topicList, receiver.TopicFilters);
        }

        [TestMethod]
        [DataRow("testQueue", "testTopic")]
        [DataRow("foo", "bar")]
        [DataRow("foo.test", "bar.test")]
        public void AddingKeyToDictionaryWorks(string queueName, string topic)
        {
            // Arrange
            var context = new TestBusContext();
            var key = new TestBusKey(queueName, topic);
            
            var wrapper = new TestBusQueueWrapper<EventMessage>();
            
            // Act
            context.DataQueues[key] = wrapper;
            
            // Assert
            Assert.AreSame(wrapper, context.DataQueues[key]);
        }

        [TestMethod]
        public void ConnectionThrowsNotImplementedException()
        {
            // Arrange
            var context = new TestBusContext();

            IConnection dummyVariable = null;
            
            // Act
            void Act() => dummyVariable = context.Connection;
            
            // Assert
            Assert.ThrowsException<NotImplementedException>(Act);
        }
        
        [TestMethod]
        public void ExchangeNameThrowsNotImplementedException()
        {
            // Arrange
            var context = new TestBusContext();

            string dummyVariable = null;
            
            // Act
            void Act() => dummyVariable = context.ExchangeName;
            
            // Assert
            Assert.ThrowsException<NotImplementedException>(Act);
        }
    }
}