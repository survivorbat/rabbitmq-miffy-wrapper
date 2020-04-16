using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Miffy.TestBus;

namespace Miffy.Test.Unit.TestBus
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
    }
}
